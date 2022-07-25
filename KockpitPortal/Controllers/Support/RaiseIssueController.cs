using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.Cart;
using KockpitPortal.Models.Notification;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.Models.Support;
using KockpitPortal.ViewModels.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Controllers.Support
{
    public class RaiseIssueController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        GetCurrentDateTime currentDateTime;
        public RaiseIssueController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            _baseEnv = env;
            _config = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
            _oData = oDataAuthenticator;
            _LocalDS = LocalDS;
            currentDateTime = new GetCurrentDateTime(config);

        }
        public IActionResult Index(int? id,string tType="All")
        {
            if (!ISADMIN && !ISSUBADMIN) return RedirectToAction("Logout", "Login");
            SubAdminDashboard();
            //Manage Notifications
            if (id != null)
            {
                string data = HttpContext.Session.GetString("Notification");
                if (data != "[]")
                {
                    var res = oDataAuthenticator.UpdateNotification(id);
                    if (res)
                    {
                        List<tblNotification> notifications = JsonConvert.DeserializeObject<List<tblNotification>>(data);
                        int i = 0;
                        foreach (tblNotification _notification in notifications)
                        {
                            if (_notification.Id == id)
                            {
                                notifications.RemoveAt(i);
                                break;
                            }
                            else
                            {
                                i++;
                                continue;
                            }
                        }
                        string notify = JsonConvert.SerializeObject(notifications);
                        HttpContext.Session.SetString("Notification", notify);
                    }
                }
            }

            //Manage Cart
            List<tblCart> _carts = new List<tblCart>();
            oDataAuthenticator.GetProductsFromCart(USERID, "tCart");
            if (this.LocalDS.Tables["tCart"] != null && this.LocalDS.Tables["tCart"].Rows.Count > 0)

            {
                foreach (DataRow dr in this.LocalDS.Tables["tCart"].Rows)

                {
                    _carts.Add(new tblCart
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        OfferingName = dr["projectname"].ToString().Trim(),
                        PlanName = dr["planname"].ToString().Trim(),
                        Price = Convert.ToInt32(dr["price"].ToString()),
                        TotalLicense = Convert.ToInt32(dr["totallicense"].ToString()),
                    });
                }

            }

            string products = JsonConvert.SerializeObject(_carts);
            HttpContext.Session.SetString("Prodcucts", products);

            //----------------- Fetch Tickets---------------------
            tblTicketViewModel tblTicketView = new tblTicketViewModel();
            List<tblTicket> tickets = new List<tblTicket>();


            oDataAuthenticator.GetTicketsforClient(USERID, "tTickets");

            if (this.LocalDS.Tables["tTickets"] != null &&
               this.LocalDS.Tables["tTickets"].Rows.Count > 0)
            {
                tickets = this.LocalDS.Tables["tTickets"].AsEnumerable().Select(row =>
                new tblTicket
                {
                    Id = row.Field<int>("id"),
                    TicketSubject = row.Field<string>("ticketsubject"),
                    UniqueId = row.Field<string>("uniqueid"),
                    AssignedTo = !string.IsNullOrEmpty(row.Field<int?>("assignedto").ToString()) ? row.Field<int?>("assignedto") : 0,
                    CreatedOn = row.Field<DateTime>("createdon"),
                    Level = row.Field<string>("levelname"),
                    Description = row.Field<string>("description"),
                    TicketStatus = row.Field<string>("ticketstatus"),
                    IssueId = row.Field<int>("issueid"),
                    LevelId = row.Field<int>("levelid"),
                    subscriptionId = row.Field<string>("subscriptionid"),
                    UserName = row.Field<string>("companyname"),
                    Scenario = row.Field<string>("scenario"),
                    Resolution = !string.IsNullOrEmpty(row.Field<string>("resolution")) ? row.Field<string>("resolution") : "",
                    UserId = row.Field<int>("userid"),
                    Attachment = !string.IsNullOrEmpty(row.Field<string>("attachment"))?row.Field<string>("attachment"): ""
                }).ToList();
            }


            tblTicketView.tickets = tickets;

            ViewBag.Plans = GetPlans();
            ViewBag.Issue = GetIssueType();

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageTitle"] = "Support";
            ViewData["PageId"] = "OA006";
            ViewBag.TicketType = tType;
            return View(tblTicketView);
        }


        [HttpPost]
        public IActionResult Index(tblTicketViewModel ticketViewModel)
        {
            var plan = new tblPlan();
            var allow = true;
            var nUniqueTicketId = "";
            List<tblUsers> users = new List<tblUsers>();
            var strFilePath = "";
            bool success = false;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    //-------------------Get Ticket Id-----------------

                    var TicketId = CreateTicketId();
                    if (TicketId.Item1)
                    {
                        nUniqueTicketId = TicketId.Item2;
                    }
                    else
                    {
                        allow = false;
                        TempData["error"] = TicketId.Item2;
                    }
                    var serviceId = ticketViewModel.ticket.subscriptionId.Substring(0, ticketViewModel.ticket.subscriptionId.LastIndexOf("-"));
                    var isService = ticketViewModel.ticket.subscriptionId.Substring(ticketViewModel.ticket.subscriptionId.LastIndexOf("-")+1).ToLower() == "service" ? true : false;
                    #region Service
                    if (isService)
                    {
                        var CheckAMC = IsAMCExpire(serviceId);///Service 
                        if (CheckAMC.Item1)
                        {
                            plan = CheckAMC.Item3;
                            var IsAllowed = CheckServiceAMC(serviceId, plan.ProjectId);
                            if (IsAllowed.Item1)
                            {
                                var nManagers = GetSupportManagers();
                                if (String.IsNullOrEmpty(nManagers.Item1))
                                {
                                    users = nManagers.Item2;

                                    // ----------------------------- Save Ticket --------------------------------
                                    if (ticketViewModel.ticket.file != null && ticketViewModel.ticket.file.Length > 0)
                                    {
                                        //Save Ticket To Physical Location
                                        var fileLocation = UploadImage(ticketViewModel.ticket.file, nUniqueTicketId);
                                        if (fileLocation.Item1)
                                        {
                                            ticketViewModel.ticket.Attachment = fileLocation.Item2;
                                        }
                                        else
                                        {
                                            allow = false;
                                            TempData["error"] = fileLocation.Item2;
                                        }
                                    }
                                    if (allow)
                                    {
                                        if (oDataAuthenticator.SaveTicketLevel(ticketViewModel.ticket.Level, "tTicketLevel"))
                                        {
                                            if (this.LocalDS.Tables["tTicketLevel"] != null && this.LocalDS.Tables["tTicketLevel"].Rows.Count > 0)
                                            {
                                                var levelId = Convert.ToInt32(this.LocalDS.Tables["tTicketLevel"].Rows[0][0].ToString().Trim());

                                                 success = oDataAuthenticator.SaveServiceTicket(ticketViewModel.ticket.TicketSubject, nUniqueTicketId, ticketViewModel.ticket.Description, ticketViewModel.ticket.Attachment, levelId, USERID, plan.ProjectId, ticketViewModel.ticket.IssueId, ticketViewModel.ticket.Scenario, currentDateTime.CurrentDatetime().Result);
                                                
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    TempData["error"] = nManagers.Item1;
                                }
                            }
                            else
                            {
                                allow = false;
                                TempData["error"] = IsAllowed.Item2;
                            }
                        }
                        else
                        {
                            allow = false;
                            TempData["error"] = CheckAMC.Item2;
                        }
                    }
                    #endregion
                    #region Offerings
                    else
                    {

                        // -------------Check License is expired or not--------------------------
                        var CheckLicense = IsLicenseExpire(ticketViewModel.ticket.subscriptionId);///offering
                        if (CheckLicense.Item1)
                        {
                            plan = CheckLicense.Item3;
                            var IsAllowed = CheckWarrantyAndAMC(ticketViewModel.ticket.subscriptionId, plan.Id);//offering
                            if (IsAllowed.Item1)
                            {
                                var nManagers = GetSupportManagers();
                                if (String.IsNullOrEmpty(nManagers.Item1))
                                {
                                    users = nManagers.Item2;

                                    // ----------------------------- Save Ticket --------------------------------
                                    if (ticketViewModel.ticket.file != null && ticketViewModel.ticket.file.Length > 0)
                                    {

                                        //Save Ticket To Physical Location
                                        var fileLocation = UploadImage(ticketViewModel.ticket.file, nUniqueTicketId);
                                        if (fileLocation.Item1)
                                        {
                                            ticketViewModel.ticket.Attachment = fileLocation.Item2;
                                        }
                                        else
                                        {
                                            allow = false;
                                            TempData["error"] = fileLocation.Item2;
                                        }
                                    }
                                    if (allow)
                                    {
                                        if (oDataAuthenticator.SaveTicketLevel(ticketViewModel.ticket.Level, "tTicketLevel"))
                                        {
                                            if (this.LocalDS.Tables["tTicketLevel"] != null && this.LocalDS.Tables["tTicketLevel"].Rows.Count > 0)
                                            {
                                                var levelId = Convert.ToInt32(this.LocalDS.Tables["tTicketLevel"].Rows[0][0].ToString().Trim());

                                                success = oDataAuthenticator.SaveTicket(ticketViewModel.ticket.TicketSubject, nUniqueTicketId, ticketViewModel.ticket.Description, ticketViewModel.ticket.Attachment, levelId, USERID, plan.ProjectId, ticketViewModel.ticket.IssueId, ticketViewModel.ticket.Scenario, currentDateTime.CurrentDatetime().Result);
                                                
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    TempData["error"] = nManagers.Item1;
                                }
                            }
                            else
                            {
                                allow = false;
                                TempData["error"] = IsAllowed.Item2;
                            }
                        }
                        else
                        {
                            allow = false;
                            TempData["error"] = CheckLicense.Item2;
                        }

                    }
                    #endregion

                    if (success)
                    {
                        var message = "ticket logged by client " + HttpContext.Session.GetString("SessionInfo_CompanyName");

                        foreach (var user in users)
                        {
                            //insert notification
                            oDataAuthenticator.InsertNotification("New ticket logged", message, "MESSAGE", currentDateTime.CurrentDatetime().Result, user.Id);

                            //send mail to support manager
                            SendMailNotificationOnTicketAction(user.EmailId, 1, plan.UserName, plan.ProjectName);
                            //SendMail(user.EmailId, sMailSubject, sMailBody, null, out sMailOutput);

                            // send whatsapp to support manager
                            if (!string.IsNullOrEmpty(user.ContactNo1))
                            {
                                SendWhatsappNotification(1, user.ContactNo1, nUniqueTicketId, user.CompanyName, ticketViewModel.ticket.TicketSubject);
                            }
                        }

                        //send whatsapp to client itself
                        if (!string.IsNullOrEmpty(WHATSAPPNUMBER))
                        {
                            SendWhatsappNotification(2, "", nUniqueTicketId, COMPANYNAME, ticketViewModel.ticket.TicketSubject);
                        }
                        if (!string.IsNullOrEmpty(SUPPORTEMAIL))
                        {
                            //Send Email to support@kockpit.in
                            SendMailNotificationOnTicketAction(SUPPORTEMAIL, 1, plan.UserName, plan.ProjectName);
                        }

                        TempData["success"] = "succesfully created a ticket";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    if (System.IO.File.Exists(strFilePath))
                        System.IO.File.Delete(strFilePath);
                    transaction.Dispose();
                    TempData["error"] = e.Message;
                }
                catch (Exception e)
                {
                    if (System.IO.File.Exists(strFilePath))
                        System.IO.File.Delete(strFilePath);
                    transaction.Dispose();
                    TempData["error"] = e.Message;
                }
            }
            return RedirectToAction("Index");
        }



        public IActionResult ResolveIssue(int? id, string tType = "All")
        {
            if (!ISSUPPORTREPRESENTATIVE) return RedirectToAction("Logout", "Login");
            

            SupportRepDashboard();

            if (id != null)
            {
                string data = HttpContext.Session.GetString("Notification");
                if (data != "[]")
                {
                    var res = oDataAuthenticator.UpdateNotification(id);
                    if (res)
                    {
                        List<tblNotification> notifications = JsonConvert.DeserializeObject<List<tblNotification>>(data);
                        int i = 0;
                        foreach (tblNotification _notification in notifications)
                        {
                            if (_notification.Id == id)
                            {
                                notifications.RemoveAt(i);
                                break;
                            }
                            else
                            {
                                i++;
                                continue;
                            }
                        }
                        string notify = JsonConvert.SerializeObject(notifications);
                        HttpContext.Session.SetString("Notification", notify);
                    }
                }
            }

            List<tblTicket> tickets = new List<tblTicket>();
            oDataAuthenticator.GetTicketsforSupportPerson("tTicketsList");
            if (this.LocalDS.Tables["tTicketsList"] != null &&
                 this.LocalDS.Tables["tTicketsList"].Rows.Count > 0)
            {
                tickets = this.LocalDS.Tables["tTicketsList"].AsEnumerable().Select(row =>
                new tblTicket
                {
                    Id = row.Field<int>("id"),
                    TicketSubject = row.Field<string>("ticketsubject"),
                    UniqueId = row.Field<string>("uniqueid"),
                    AssignedTo = !string.IsNullOrEmpty(row.Field<int?>("assignedto").ToString()) ? row.Field<int?>("assignedto") : 0,
                    CreatedOn = row.Field<DateTime>("createdon"),
                    Level = row.Field<string>("levelname"),
                    Description = row.Field<string>("description"),
                    TicketStatus = row.Field<string>("ticketstatus"),
                    IssueId = row.Field<int>("issueid"),
                    LevelId = row.Field<int>("levelid"),
                    subscriptionId = row.Field<string>("subscriptionid"),
                    UserName = row.Field<string>("companyname"),
                    Scenario = row.Field<string>("scenario"),
                    Resolution = !string.IsNullOrEmpty(row.Field<string>("resolution")) ? row.Field<string>("resolution") : "",
                    UserId = row.Field<int>("userid"),
                    Attachment = !string.IsNullOrEmpty(row.Field<string>("attachment"))? row.Field<string>("attachment") :""
                }).ToList();
            }
            ViewBag.Plans = GetPlansSupport();
            ViewBag.Issue = GetIssueType();
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            tblTicketViewModel tblTicketView = new tblTicketViewModel();
            tblTicketView.tickets = tickets;
            ViewBag.SupportManagerID = USERID;
            ViewData["PageId"] = "OA002";
            ViewData["PageTitle"] = "Support";
            ViewBag.TicketType = tType;
            return View(tblTicketView);
        }

        private void SubAdminDashboard()
        {
            List<tblNotification> notifications1 = new List<tblNotification>();
            int totalAssignedToMe = 0, total = 0, TotalCloseByMe = 0, unassigned = 0;
            _oData.GetSubAdminDashboard(USERID, "tSupportRepDashboard");
            if (this.LocalDS.Tables["tSupportRepDashboard"] != null && this.LocalDS.Tables["tSupportRepDashboard"].Rows.Count > 0)
            {
                totalAssignedToMe = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard1"] != null && this.LocalDS.Tables["tSupportRepDashboard1"].Rows.Count > 0)
            {
                TotalCloseByMe = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard1"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard2"] != null && this.LocalDS.Tables["tSupportRepDashboard2"].Rows.Count > 0)
            {
                total = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard2"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard3"] != null && this.LocalDS.Tables["tSupportRepDashboard3"].Rows.Count > 0)
            {
                unassigned = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard3"].Rows[0][0].ToString().Trim());
            }
            _oData.GetNotificationForSupport(USERID, "tNotifications");
            if (this.LocalDS.Tables["tNotifications"] != null && this.LocalDS.Tables["tNotifications"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tNotifications"].Rows)
                {
                    notifications1.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }
            string notify1 = JsonConvert.SerializeObject(notifications1);
            HttpContext.Session.SetString("Notification", notify1);
            ViewBag.Unassigned = unassigned;
            ViewBag.TotalAssignedToMe = totalAssignedToMe;
            ViewBag.TotalCloseByMe = TotalCloseByMe;
            ViewBag.Total = total;
            ViewData["PageId"] = "OA001";
        }

        private void SupportRepDashboard()
        {
            List<tblNotification> notifications1 = new List<tblNotification>();
            int totalAssignedToMe = 0, total = 0, TotalCloseByMe = 0, unassigned = 0;
            _oData.GetSupportRepDashboard(USERID, "tSupportRepDashboard");
            if (this.LocalDS.Tables["tSupportRepDashboard"] != null && this.LocalDS.Tables["tSupportRepDashboard"].Rows.Count > 0)
            {
                totalAssignedToMe = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard1"] != null && this.LocalDS.Tables["tSupportRepDashboard1"].Rows.Count > 0)
            {
                TotalCloseByMe = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard1"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard2"] != null && this.LocalDS.Tables["tSupportRepDashboard2"].Rows.Count > 0)
            {
                total = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard2"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportRepDashboard3"] != null && this.LocalDS.Tables["tSupportRepDashboard3"].Rows.Count > 0)
            {
                unassigned = Convert.ToInt32(this.LocalDS.Tables["tSupportRepDashboard3"].Rows[0][0].ToString().Trim());
            }
            _oData.GetNotificationForSupport(USERID, "tNotifications");
            if (this.LocalDS.Tables["tNotifications"] != null && this.LocalDS.Tables["tNotifications"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tNotifications"].Rows)
                {
                    notifications1.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }
            string notify1 = JsonConvert.SerializeObject(notifications1);
            HttpContext.Session.SetString("Notification", notify1);
            ViewBag.Unassigned = unassigned;
            ViewBag.TotalAssignedToMe = totalAssignedToMe;
            ViewBag.TotalCloseByMe = TotalCloseByMe;
            ViewBag.Total = total;
            ViewData["PageId"] = "OA001";
        }

        public IActionResult UpdateTicketStatus(string status, string resolution, int ticketId)
        {
            var response = new Response();
            tblUsers users = new tblUsers();
            tblTicket tblTicket = new tblTicket();

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res = oDataAuthenticator.CloseTicket(status, USERID, resolution, ticketId, currentDateTime.CurrentDatetime().Result);
                    if (res)
                    {

                        var ticketDetail =  GetTicketDetails(ticketId);
                        if (string.IsNullOrEmpty(ticketDetail.Item1))
                        {
                            tblTicket = ticketDetail.Item2;

                            var UserDetail = GetUserDetailbyId(tblTicket.UserId);
                            if(String.IsNullOrEmpty(UserDetail.Item1))
                            {
                                users = UserDetail.Item2;

                                string message = "ticket closed by " + HttpContext.Session.GetString("SessionInfo_CompanyName");

                                SendMailNotificationOnTicketAction(users.EmailId, 4, "", "", tblTicket.UniqueId, message);


                                oDataAuthenticator.InsertNotification(("Update on ticket no " + tblTicket.UniqueId + ""), message, "MESSAGE", currentDateTime.CurrentDatetime().Result, tblTicket.UserId);
                                if (!string.IsNullOrEmpty(users.ContactNo1))
                                {
                                    SendWhatsappNotification(5, users.ContactNo1, tblTicket.UniqueId, users.CompanyName, tblTicket.TicketSubject);
                                }
                            }
                            else
                            {
                                TempData["error"] = UserDetail.Item1;
                            }

                        }
                        else
                        {
                            TempData["error"] = ticketDetail.Item1;
                        }
                        TempData["success"] = "Ticket closed succesfully";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        TempData["error"] = "Ticket can not close";
                        transaction.Dispose();
                    }

                }
                catch (TransactionException e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
            }
            if (ISSUPPORTMANAGER)
                return RedirectToAction("ShowAllTickets");
            else if (ISSUPPORTREPRESENTATIVE)
                return RedirectToAction("ResolveIssue");
            else
                return RedirectToAction("Login", "Logout");
        }

        public JsonResult UpdateTicketResolutionLevel(int tId, string ticketLevel, string nResolution)
        {
            var response = new Response();
            response.success = false;
            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oDataAuthenticator.SaveTicketLevel(ticketLevel, "tTicketLevel");
                    if (this.LocalDS.Tables["tTicketLevel"] != null && this.LocalDS.Tables["tTicketLevel"].Rows.Count > 0)
                    {
                        var levelId = Convert.ToInt32(this.LocalDS.Tables["tTicketLevel"].Rows[0][0].ToString().Trim());
                        if (oDataAuthenticator.UpdateTicketBySupport(levelId, nResolution, tId, currentDateTime.CurrentDatetime().Result))
                        {
                            transaction.Complete();
                            transaction.Dispose();
                            response.success = true;
                        }
                    }

                }
                catch (TransactionException e)
                {
                    transaction.Dispose();
                    response.msg = e.Message;
                }
                catch (Exception e)
                {
                    transaction.Dispose();
                    response.msg = e.Message;
                }
            }
            return Json(response);
        }

        public IActionResult ShowAllTickets(int? id, string tType = "All")
        {
            if (!ISSUPPORTMANAGER) return RedirectToAction("Logout", "Login");
            SupportManagerDashboard();
            if (id != null)
            {
                string data = HttpContext.Session.GetString("Notification");
                if (data != "[]")
                {
                    var res = oDataAuthenticator.UpdateNotification(id);
                    if (res)
                    {
                        List<tblNotification> notifications = JsonConvert.DeserializeObject<List<tblNotification>>(data);
                        int i = 0;
                        foreach (tblNotification _notification in notifications)
                        {
                            if (_notification.Id == id)
                            {
                                notifications.RemoveAt(i);
                                break;
                            }
                            else
                            {
                                i++;
                                continue;
                            }
                        }
                        string notify = JsonConvert.SerializeObject(notifications);
                        HttpContext.Session.SetString("Notification", notify);
                    }
                }
            }

            List<tblTicket> tickets = new List<tblTicket>();
            oDataAuthenticator.GetTicketsforSupportPerson("tTicketsList");
            if (this.LocalDS.Tables["tTicketsList"] != null &&
                this.LocalDS.Tables["tTicketsList"].Rows.Count > 0)
            {
                tickets = this.LocalDS.Tables["tTicketsList"].AsEnumerable().Select(row =>
                new tblTicket
                {
                    Id = row.Field<int>("id"),
                    TicketSubject = row.Field<string>("ticketsubject"),
                    UniqueId = row.Field<string>("uniqueid"),
                    AssignedTo = !string.IsNullOrEmpty(row.Field<int?>("assignedto").ToString()) ? row.Field<int?>("assignedto") : 0,
                    CreatedOn = row.Field<DateTime>("createdon"),
                    Level = row.Field<string>("levelname"),
                    Description = row.Field<string>("description"),
                    TicketStatus = row.Field<string>("ticketstatus"),
                    IssueId = row.Field<int>("issueid"),
                    LevelId = row.Field<int>("levelid"),
                    subscriptionId = row.Field<string>("subscriptionid"),
                    UserName = row.Field<string>("companyname"),
                    Scenario = row.Field<string>("scenario"),
                    Resolution = !string.IsNullOrEmpty(row.Field<string>("resolution")) ? row.Field<string>("resolution") : "",
                    UserId = row.Field<int>("userid"),
                    Attachment = !string.IsNullOrEmpty(row.Field<string>("attachment"))? row.Field<string>("attachment"):""
                }).ToList();
            }


            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewBag.Plans = GetPlansSupport();
            ViewBag.Issue = GetIssueType();
            ViewBag.Employees = GetEmpoloyeeList();
            tblTicketViewModel tblTicketView = new tblTicketViewModel();
            tblTicketView.tickets = tickets;
            ViewBag.SupportManagerID = USERID;
            ViewData["PageId"] = "OA002";
            ViewData["PageTitle"] = "Support";
            ViewBag.TicketType = tType;
            return View(tblTicketView);
        }

        private void SupportManagerDashboard()
        {
            int totalAssignedToMe = 0, totalPending = 0, TotalCloseByMe = 0, AssignedtoOther = 0;
            List<tblNotification> notifications = new List<tblNotification>();
            _oData.GetNotificationForSupport(USERID, "tNotification");

            if (this.LocalDS.Tables["tNotification"] != null && this.LocalDS.Tables["tNotification"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tNotification"].Rows)
                {
                    notifications.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }
            _oData.GetSupportManagerDashboard(USERID, "tSupportManagerDashboard");
            if (this.LocalDS.Tables["tSupportManagerDashboard"] != null && this.LocalDS.Tables["tSupportManagerDashboard"].Rows.Count > 0)
            {
                totalPending = Convert.ToInt32(this.LocalDS.Tables["tSupportManagerDashboard"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportManagerDashboard1"] != null && this.LocalDS.Tables["tSupportManagerDashboard1"].Rows.Count > 0)
            {
                totalAssignedToMe = Convert.ToInt32(this.LocalDS.Tables["tSupportManagerDashboard1"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportManagerDashboard2"] != null && this.LocalDS.Tables["tSupportManagerDashboard2"].Rows.Count > 0)
            {
                TotalCloseByMe = Convert.ToInt32(this.LocalDS.Tables["tSupportManagerDashboard2"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSupportManagerDashboard3"] != null && this.LocalDS.Tables["tSupportManagerDashboard3"].Rows.Count > 0)
            {
                AssignedtoOther = Convert.ToInt32(this.LocalDS.Tables["tSupportManagerDashboard3"].Rows[0][0].ToString().Trim());
            }
            string notify = JsonConvert.SerializeObject(notifications);
            HttpContext.Session.SetString("Notification", notify);
            ViewBag.TotalPending = totalPending;
            ViewBag.TotalAssignedToMe = totalAssignedToMe;
            ViewBag.TotalCloseByMe = TotalCloseByMe;
            ViewBag.AssignedtoOthers = AssignedtoOther;
            ViewData["PageId"] = "OA001";
        }

        public IActionResult AssignTicket(int id, string remarks, int empId)
        {
            tblTicket ticketDetail = new tblTicket();
            tblUsers users = new tblUsers();
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {

                    var userDetails = GetUserDetailbyId(empId);
                    if (string.IsNullOrEmpty(userDetails.Item1))
                    {
                        users = userDetails.Item2;

                        var res = oDataAuthenticator.UpdateTicketStatus("Open", empId, remarks, id, USERID, currentDateTime.CurrentDatetime().Result);

                        if (res)
                        {
                            if (empId != USERID)
                            {

                                var ticketDetails = GetTicketDetails(id);
                                if (string.IsNullOrEmpty(ticketDetails.Item1))
                                {
                                    ticketDetail = ticketDetails.Item2;

                                    string message = "ticket assigned to you by " + HttpContext.Session.GetString("SessionInfo_CompanyName");

                                    SendMailNotificationOnTicketAction(users.EmailId, 2, "", "", ticketDetail.UniqueId, "");

                                    oDataAuthenticator.InsertNotification(("New ticket assigned to you " + ticketDetail.UniqueId + ""), message, "MESSAGE", currentDateTime.CurrentDatetime().Result,empId);
                                    if (!string.IsNullOrEmpty(users.ContactNo1))
                                    {
                                        SendWhatsappNotification(3, users.ContactNo1, ticketDetail.UniqueId, users.CompanyName, ticketDetail.TicketSubject);
                                    }
                                }
                                else
                                {
                                    TempData["error"] = ticketDetails.Item1;
                                    transaction.Dispose();
                                }
                            }

                            TempData["success"] = "successfully assign the ticket to employee";
                            transaction.Complete();
                            transaction.Dispose();

                        }
                        else
                        {
                            transaction.Dispose();
                            TempData["error"] = "something went wrong while assigning this ticket to employee";
                        }
                    }

                    else
                    {
                        TempData["error"] = userDetails.Item1;
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
            }
            return RedirectToAction("ShowAllTickets");
        }

        public JsonResult AddComment(int tTicketId, int nAssignedTo, string tComment, IFormFile tAttachment, string userId, string uniqueId)
        {
            var response = new Response();
            response.success = false;
            var allow = true;
            tblUsers users = new tblUsers();
            var strFilePath = "";
            tblTicket ticket = new tblTicket();
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {

                    if (tAttachment != null && tAttachment.Length > 0)
                    {
                        var nLocation = UploadImage(tAttachment, uniqueId);
                        if (nLocation.Item1)
                        {
                            strFilePath = nLocation.Item2;
                        }
                        else
                        {
                            allow = false;
                            response.msg = nLocation.Item2;
                        }
                    }
                    if (allow)
                    {
                        var res = oDataAuthenticator.AddComment(tTicketId, USERID, nAssignedTo, tComment, strFilePath, currentDateTime.CurrentDatetime().Result);
                        if (res)
                        {

                            var ticketDetail = GetTicketDetails(tTicketId);
                            if (string.IsNullOrEmpty(ticketDetail.Item1))
                            {
                                ticket = ticketDetail.Item2;


                                var userID = ticket.UserId == USERID ? ticket.AssignedTo.Value : ticket.UserId;

                                var userDetails = GetUserDetailbyId(userID);
                                if (string.IsNullOrEmpty(userDetails.Item1))
                                {
                                    users = userDetails.Item2;

                                    string message = "comment added on ticket  by " + HttpContext.Session.GetString("SessionInfo_CompanyName");
                                    oDataAuthenticator.InsertNotification(("Update on ticket no " + ticket.UniqueId + ""), message, "MESSAGE", currentDateTime.CurrentDatetime().Result,users.Id);


                                    SendMailNotificationOnTicketAction(users.EmailId, 3, "", "", ticket.UniqueId, message);
                                    if (!string.IsNullOrEmpty(users.ContactNo1))
                                    {
                                        SendWhatsappNotification(4, users.ContactNo1, uniqueId, users.CompanyName, ticket.TicketSubject);
                                    }
                                    response.success = true;
                                    transaction.Complete();
                                    transaction.Dispose();

                                }
                                else
                                {
                                    response.msg = userDetails.Item1;
                                }
                            }
                            else
                            {
                                if (strFilePath != null)
                                {
                                    FileInfo uploadedFile = new FileInfo(strFilePath);
                                    uploadedFile.Delete();
                                }
                                response.msg = "can not add comment please try again";
                            }
                        }
                    }
                }
                catch (TransactionException e)
                {
                    if (strFilePath != null)
                    {
                        FileInfo uploadedFile = new FileInfo(strFilePath);
                        uploadedFile.Delete();
                    }
                    response.msg = e.Message;
                }
                catch (Exception e)
                {
                    if (strFilePath != null)
                    {
                        FileInfo uploadedFile = new FileInfo(strFilePath);
                        uploadedFile.Delete();
                    }
                    response.msg = e.Message;
                }
            }
            return Json(response);
        }

        public IActionResult TimeLine(int id)
        {
            if (ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            List<tblTicketHistory> tHistory = new List<tblTicketHistory>();

            oDataAuthenticator.TimelineByTicketId(id, "tTicketId");
            if (this.LocalDS.Tables["tTicketId"] != null
                && this.LocalDS.Tables["tTicketId"].Rows.Count > 0)
            {
                tHistory = this._LocalDS.Tables["tTicketId"].AsEnumerable().Select(row =>
                new tblTicketHistory
                {
                    Id = row.Field<int>("id"),
                    Discription = row.Field<string>("description"),
                    TicketStatus = row.Field<string>("ticketstatus"),
                    TicketId = row.Field<int>("ticketid"),
                    AssignedUser = row.Field<string>("assigneduser"),
                    UserName = row.Field<string>("username"),
                    Logo = string.IsNullOrEmpty(row.Field<string>("logo")) ? "~/assets/download.png" : row.Field<string>("logo"),
                    Remarks = row.Field<string>("remarks"),
                    Attachment = row.Field<string>("attachment"),
                    TicketStatusDate = row.Field<DateTime>("ticketstatusdate"),
                    Resolution = row.Field<string>("resolution"),
                    Action = row.Field<string>("action"),
                }).ToList();
                if (ISADMIN || ISSUBADMIN && tHistory.Count > 0)
                {
                    for (int i = 0; i < tHistory.Count(); i++)
                    {
                        if (string.Equals(tHistory[i].Action.Trim(), "Ticket assigned".Trim(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            tHistory.RemoveAt(i);
                            break;
                        }
                    }
                }
                ViewBag.TimeLine = tHistory;
            }
            ViewData["PageTitle"] = "Support";
            return View();
        }

        public JsonResult ShowComments(int id)
        {
            List<tblTicketHistory> tHistory = new List<tblTicketHistory>();
            var response = new Response();
            response.success = false;

            try
            {
                oDataAuthenticator.TimelineByTicketId(id, "tTicketId");
                if (this.LocalDS.Tables["tTicketId"] != null
                    && this.LocalDS.Tables["tTicketId"].Rows.Count > 0)
                {
                    tHistory = this._LocalDS.Tables["tTicketId"].AsEnumerable().Select(row =>
                    new tblTicketHistory
                    {
                        Id = row.Field<int>("id"),
                        Discription = row.Field<string>("description"),
                        TicketStatus = row.Field<string>("ticketstatus"),
                        TicketId = row.Field<int>("ticketid"),
                        AssignedUser = row.Field<string>("assigneduser"),
                        UserName = row.Field<string>("username"),
                        Logo = string.IsNullOrEmpty(row.Field<string>("logo")) ? "~/assets/download.png" : row.Field<string>("logo"),
                        Remarks = row.Field<string>("remarks"),
                        Attachment = row.Field<string>("attachment"),
                        TicketStatusDate = row.Field<DateTime>("ticketstatusdate"),
                        Resolution = row.Field<string>("resolution"),
                        Action = row.Field<string>("action"),
                    }).ToList();
                    response.success = true;
                }
            }
            catch (Exception e)
            {
                response.msg = e.Message;
            }
            return Json(new Tuple<Response, List<tblTicketHistory>>(response, tHistory));
        }

        public void SendWhatsappNotification(int ticketAction = 0, string mobileNum = "", string ticketId = "", string nRecieverName = "", string ticketSubject = "")
        {
            try
            {
                mobileNum = "91" + mobileNum;
                var CurrentUserNum = "91" + WHATSAPPNUMBER;
                switch (ticketAction)
                {
                    case 1:
                        SendWhatsappMethod(mobileNum, nRecieverName, ticketId, ticketSubject, "ticket_logged_support", COMPANYNAME);
                        break;
                    case 2:
                        SendWhatsappMethod(CurrentUserNum, COMPANYNAME, ticketId, ticketSubject, "ticked_logged");
                        break;
                    case 3:
                        SendWhatsappMethod(mobileNum, nRecieverName, ticketId, COMPANYNAME);
                        break;
                    case 4:
                        SendWhatsappMethod(mobileNum, nRecieverName, ticketId,"", "update_ticket","");
                        break;
                    case 5:
                        SendWhatsappMethod(mobileNum, nRecieverName, ticketId,"", "ticket_closed","");
                        break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                //log
            }
        }

        public void SendMailNotificationOnTicketAction(string EmailId, int ticketaction = 0, string userName = "", string projectName = "", string uniqueId = "", string msg = "")
        {
            string sMailSubject = "";
            string sMailOutput = "";
            string sMailBody = "";
            if (ticketaction == 1) // When Ticket Logged
            {
                string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Ticket.html")))
                                            ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Ticket.html"))
                                                : string.Empty;

                sMailSubject = "Ticket Raised";
                sMailBody = string.Format(mailBody.ToString(), userName, projectName);
            }

            else if (ticketaction == 2)// When ticket is assigned to support Rep
            {
                string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailtosupportrep.html")))
                                            ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailtosupportrep.html"))
                                                : string.Empty;

                sMailOutput = "";
                sMailSubject = "Ticket Assigned";
                sMailBody = string.Format(mailBody.ToString(), uniqueId, HttpContext.Session.GetString("SessionInfo_CompanyName"));

            }
            else if (ticketaction == 3)//When comment added on ticket
            {
                sMailSubject = "New Comment Added on Ticket (" + uniqueId + ")";
                sMailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Comment.html")))
                             ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Comment.html"))
                                 : string.Empty;
                sMailBody = string.Format(sMailBody.ToString(), msg);
            }
            else if (ticketaction == 4)// When Ticket is Closed
            {
                sMailSubject = "Ticket Closed (" + uniqueId + ")";
                sMailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Comment.html")))
                            ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "Comment.html"))
                                : string.Empty;
                sMailBody = string.Format(sMailBody.ToString(), msg);
            }

            SendMail(EmailId, sMailSubject, sMailBody, null, out sMailOutput);
        }

        public Tuple<bool, string> CreateTicketId()
        {
            string uniqueId = "";
            var res = true;
            try
            {
                var mm = DateTime.Now.Month.ToString("00");
                var dd = DateTime.Now.ToString("dd");
                var yy = DateTime.Now.ToString("yyyy");
                var id = 1;
                oDataAuthenticator.GetLastInsertedId("tLastTicketId");
                if (this.LocalDS.Tables["tLastTicketId"] != null
                    && this.LocalDS.Tables["tLastTicketId"].Rows.Count > 0)
                {
                    id = Convert.ToInt32(this.LocalDS.Tables["tLastTicketId"].Rows[0][0].ToString());
                    id++;
                    uniqueId = "#" + id + dd + mm + yy;
                }
                else
                {
                    uniqueId = uniqueId = "01" + id + dd + mm + yy; 
                }
            }
            catch (Exception e)
            {
                res = false;
                uniqueId = e.Message;
            }
            return Tuple.Create(res, uniqueId);
        }

        public Tuple<bool, string, tblPlan> IsLicenseExpire(string subscriptionId)
        {
            var res = false;
            var msg = "";
            tblPlan plan = new tblPlan();
            try
            {
                oDataAuthenticator.GetLicense(USERID, subscriptionId, "tLicense");
                if (this.LocalDS.Tables["tLicense"] != null && this.LocalDS.Tables["tLicense"].Rows.Count > 0)
                {
                    plan = this.LocalDS.Tables["tLicense"].AsEnumerable().Select(row => new tblPlan
                    {
                        Id = row.Field<int>("planid"),
                        UserEmail = row.Field<string>("clientEmail"),
                        ProjectId = row.Field<int>("projectId"),
                        UserName = row.Field<string>("companyname"),
                        ProjectName = row.Field<string>("projectname"),
                    }).FirstOrDefault();
                    res = true;
                }
                else
                {
                    msg = "no active license found against this offering";
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return Tuple.Create(res, msg, plan);
        }

        public Tuple<bool, string, tblPlan> IsAMCExpire(string subscriptionId)
        {
            var res = false;
            var msg = "";
            tblPlan plan = new tblPlan();
            try
            {
                oDataAuthenticator.GetAMC(USERID, subscriptionId, "tAMCExpire");
                if (this.LocalDS.Tables["tAMCExpire"] != null && this.LocalDS.Tables["tAMCExpire"].Rows.Count > 0)
                {
                    plan = this.LocalDS.Tables["tAMCExpire"].AsEnumerable().Select(row => new tblPlan
                    {   
                        UserEmail = row.Field<string>("clientEmail"),
                        ProjectId = row.Field<int>("projectId"),
                        UserName = row.Field<string>("companyname"),
                        ProjectName = row.Field<string>("projectname"),
                    }).FirstOrDefault();
                    res = true;
                }
                else
                {
                    msg = "no active AMC found against this Service";
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return Tuple.Create(res, msg, plan);
        }

        public Tuple<bool, string> CheckWarrantyAndAMC(string subscriptionId, int planId)
        {
            var res = true;
            var msg = "";
            try
            {
                oDataAuthenticator.CheckWarrantyOfPlan(subscriptionId, USERID, "tCheckWarranty");
                if (this.LocalDS.Tables["tCheckWarranty"] == null || this.LocalDS.Tables["tCheckWarranty"].Rows.Count == 0)
                {
                    oDataAuthenticator.CheckAMC(USERID, planId, subscriptionId, "tCheckAMC");
                    if (this.LocalDS.Tables["tCheckAMC"] == null || this.LocalDS.Tables["tCheckAMC"].Rows.Count == 0)
                    {
                        res = false;
                        msg = "no active amc or warranty found against this offering";
                    }
                }
            }
            catch (Exception e)
            {
                res = false;
                msg = e.Message;
            }
            return Tuple.Create(res, msg);
        }

        public Tuple<bool, string> CheckServiceAMC(string subscriptionId, int planId)
        {
            var res = true;
            var msg = "";
            try
            {
                oDataAuthenticator.CheckServiceAMC(USERID, planId, subscriptionId, "tCheckAMC");
                if (this.LocalDS.Tables["tCheckAMC"] == null || this.LocalDS.Tables["tCheckAMC"].Rows.Count == 0)
                {
                        res = false;
                        msg = "no active amc or warranty found against this offering";
                }
            }
            catch (Exception e)
            {
                res = false;
                msg = e.Message;
            }
            return Tuple.Create(res, msg);
        }

        public Tuple<string, List<tblUsers>> GetSupportManagers()
        {
            List<tblUsers> managers = new List<tblUsers>();
            var msg = "";
            try
            {
                oDataAuthenticator.GetSupportManagerEmail("tSupportManagerDetails");
                if (this.LocalDS.Tables["tSupportManagerDetails"] != null && this.LocalDS.Tables["tSupportManagerDetails"].Rows.Count > 0)
                {
                    managers = this.LocalDS.Tables["tSupportManagerDetails"].AsEnumerable().Select(row =>
                    new tblUsers
                    {
                        Id = row.Field<int>("id"),
                        EmailId = row.Field<string>("emailid"),
                        CompanyName = row.Field<string>("companyname"),
                        ContactNo1 = row.Field<string>("contactno1"),
                    }).ToList();
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return Tuple.Create(msg, managers);
        }

        public Tuple<bool, string> UploadImage(IFormFile file, string nUniqueId)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string strFilePath = "";
            string nLocation = "";
            var confirm = true;
            try
            {
                var ticketpath = "Resource/Ticket/" + USERID.ToString() + "/" + nUniqueId.Substring(1);
                strFilePath = Path.Combine(_env.WebRootPath, ticketpath, fileName);
                if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                }
                using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                    nLocation = Path.Combine("~/Resource/Ticket/" + USERID.ToString() + "/" + nUniqueId.Substring(1) + "/", fileName);
                }
            }
            catch (Exception e)
            {
                confirm = false;
                nLocation = e.Message;
            }
            return Tuple.Create(confirm, nLocation);
        }

        public Tuple<string, tblUsers> GetUserDetailbyId(int nId)
        {
            tblUsers users = new tblUsers();
            var msg = "";
            try
            {
                oDataAuthenticator.ClientUserGetById(nId, "tUsers");
                if (this.LocalDS.Tables["tUsers"] != null && this.LocalDS.Tables["tUsers"].Rows.Count > 0)
                {
                    users = this.LocalDS.Tables["tUsers"].AsEnumerable().Select(row => new tblUsers
                    {
                        Id = row.Field<int>("id"),
                        EmailId = row.Field<string>("emailid"),
                        CompanyName = row.Field<string>("companyname"),
                        ContactNo1 = row.Field<string>("contactno1"),
                    }).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return Tuple.Create(msg, users);
        }

        public Tuple<string, tblTicket> GetTicketDetails(int nTicketId)
        {
            var msg = "";
            var details = new tblTicket();
            try
            {
                oDataAuthenticator.GetTicketUserName(nTicketId, "tGetTicketDetails");
                if (this.LocalDS.Tables["tGetTicketDetails"] != null &&
                            this.LocalDS.Tables["tGetTicketDetails"].Rows.Count > 0)
                {
                    details = this.LocalDS.Tables["tGetTicketDetails"].AsEnumerable().Select(row => new tblTicket
                    {
                        UserId = row.Field<int>("userid"),
                        UniqueId = row.Field<string>("uniqueid"),
                        AssignedTo = row.Field<int>("assignedto"),
                        TicketSubject = row.Field<string>("ticketsubject"),
                    }).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return Tuple.Create(msg, details);
        }
    
    }
}



//public JsonResult GetAttachments(int tId)
//{
//    var response = new Response();
//    response.success = false;
//    List<string> attachment = new List<string>();
//    try
//    {
//        oDataAuthenticator.GetAttachments(tId, "tAttachments");
//        if (this.LocalDS.Tables["tAttachments"] != null && this.LocalDS.Tables["tAttachments"].Rows.Count > 0)
//        {
//            foreach (DataRow dr in this.LocalDS.Tables["tAttachments"].Rows)
//            {
//                if (!string.IsNullOrEmpty(dr["attachment"].ToString()))
//                    attachment.Add(dr["attachment"].ToString());
//            }
//            response.success = true;
//        }
//        else
//        {
//            response.msg = "no attachments found for this ticket...!!!";
//        }

//    }
//    catch (Exception e)
//    {
//        response.msg = e.Message;
//    }
//    return Json(new Tuple<Response, List<string>>(response, attachment));
//}

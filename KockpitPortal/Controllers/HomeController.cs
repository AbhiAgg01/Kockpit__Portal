using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KockpitPortal.Models;
using Microsoft.AspNetCore.Http;
using KockpitPortal.DataAccessLayer;
using System.Data;
using Microsoft.Extensions.Configuration;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.Models.Notification;
using KockpitPortal.Models.Cart;
using Newtonsoft.Json;
using System.Transactions;
using KockpitAuthenticator;

namespace KockpitPortal.Controllers
{
    public class HomeController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        public IConfiguration _config;
        public HomeController(IConfiguration config)
        {
            _baseConfig = config;
            _config = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
        }

        public IActionResult Index()
        {
            SuperAdminDashboard adminDashboard = new SuperAdminDashboard();
            List<tblNotification> notifications = new List<tblNotification>();
            oData.SuperAdminDashboard(USERID,"tSuperAdminDashboard");
            if(this.LocalDS.Tables["tSuperAdminDashboard"] != null && this.LocalDS.Tables["tSuperAdminDashboard"].Rows.Count > 0)
            {
                adminDashboard.TotalClients = this.LocalDS.Tables["tSuperAdminDashboard"].Rows.Count;
                adminDashboard.ClientList = this.LocalDS.Tables["tSuperAdminDashboard"].AsEnumerable().Select(row => new tblUsers { 
                    CompanyName = row.Field<string>("CompanyName"),
                    EmailId = row.Field<string>("EmailId"),
                    Logo = row.Field<string>("Logo"),
                }).ToList();
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard1"] != null && this.LocalDS.Tables["tSuperAdminDashboard1"].Rows.Count > 0)
            {
                adminDashboard.TotalEmployees += this.LocalDS.Tables["tSuperAdminDashboard1"].Rows.Count;
                adminDashboard.SupportManager = this.LocalDS.Tables["tSuperAdminDashboard1"].AsEnumerable().Select(row => new tblUsers
                {
                    CompanyName = row.Field<string>("CompanyName"),
                    EmailId = row.Field<string>("EmailId"),
                    Logo = row.Field<string>("Logo"),
                }).ToList();
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard2"] != null && this.LocalDS.Tables["tSuperAdminDashboard2"].Rows.Count > 0)
            {
                adminDashboard.TotalEmployees += this.LocalDS.Tables["tSuperAdminDashboard2"].Rows.Count;
                adminDashboard.SupportRepresentative = this.LocalDS.Tables["tSuperAdminDashboard2"].AsEnumerable().Select(row => new tblUsers
                {
                    CompanyName = row.Field<string>("CompanyName"),
                    EmailId = row.Field<string>("EmailId"),
                    Logo = row.Field<string>("Logo"),
                }).ToList();
            }

            if (this.LocalDS.Tables["tSuperAdminDashboard3"] != null && this.LocalDS.Tables["tSuperAdminDashboard3"].Rows.Count > 0)
            {
                adminDashboard.TotalLicenses = Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDashboard3"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard4"] != null && this.LocalDS.Tables["tSuperAdminDashboard4"].Rows.Count > 0)
            {
                adminDashboard.ActiveLicenses = Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDashboard4"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard5"] != null && this.LocalDS.Tables["tSuperAdminDashboard5"].Rows.Count > 0)
            {
                adminDashboard.InActiveLicenses = Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDashboard5"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard6"] != null && this.LocalDS.Tables["tSuperAdminDashboard6"].Rows.Count > 0)
            {
                adminDashboard.ExpiredLicenses = Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDashboard6"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard7"] != null && this.LocalDS.Tables["tSuperAdminDashboard7"].Rows.Count > 0)
            {
                adminDashboard.LicenseRequests = Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDashboard7"].Rows[0][0].ToString().Trim());
            }
            if (this.LocalDS.Tables["tSuperAdminDashboard8"] != null && this.LocalDS.Tables["tSuperAdminDashboard8"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tSuperAdminDashboard8"].Rows)
                {
                    notifications.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }

            string notify = JsonConvert.SerializeObject(notifications);
            HttpContext.Session.SetString("Notification", notify);

            ViewData["PageId"] = "SA001";
            return View(adminDashboard);
        }

        public IActionResult Dashboard()
        {
            List<tblProject> projects = new List<tblProject>();
            List<tblNotification> notifications = new List<tblNotification>();
            List<tblCart> _carts = new List<tblCart>();
            oData.ClientDashboard(USERID, ISADMIN,"tProjects");
            if (this.LocalDS.Tables["tProjects"] != null && this.LocalDS.Tables["tProjects"].Rows.Count > 0)
            {
                projects = this.LocalDS.Tables["tProjects"].AsEnumerable()
                    .Select(row => new tblProject
                    {
                        Id = row.Field<int>("Id"),
                        ProjectName = row.Field<string>("ProjectName"),
                        Description = row.Field<string>("Description"),
                        Version = row.Field<string>("Version"),
                        ProjectType = row.Field<string>("ProjectType"),
                        ProjectImage = row.Field<string>("ProjectImage"),
                        ProjectStartUpLink = row.Field<string>("ProjectStartUpLink"),
                        ProjectVideo = string.IsNullOrEmpty(row.Field<string>("ProjectVideo")) ? "~/assets/default.mp4" : row.Field<string>("ProjectVideo"),
                        LicenseKey = row.Field<string>("LicenseKey"),
                        ProjectIcon = row.Field<string>("ProjectIcon")
                    }).ToList();
            }

            if (this.LocalDS.Tables["tProjects1"] != null && this.LocalDS.Tables["tProjects1"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tProjects1"].Rows)
                {
                    notifications.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }

            if (this.LocalDS.Tables["tProjects2"] != null && this.LocalDS.Tables["tProjects2"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tProjects2"].Rows)
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

            string notify = JsonConvert.SerializeObject(notifications);
            HttpContext.Session.SetString("Notification", notify);

            string products = JsonConvert.SerializeObject(_carts);
            HttpContext.Session.SetString("Prodcucts", products);

            ViewBag.Projects = projects;
            ViewBag.Admin = ISADMIN;
            ViewBag.SUPPORTMANAGER = ISSUPPORTMANAGER;

            ViewData["PageId"] = "OA001";
            ViewData["PageTitle"] = "Dashboard";
            return View();
        }

        public JsonResult SetKID(string key, string pro = "0")
        {
            bool lretval = true;
            try
            {
                var jExtra = new JwtExtra {
                    kid = key,
                    kpro = pro
                };
                Response.Cookies.Append("jwt_token_spec", JsonConvert.SerializeObject(jExtra), CookieOpt);
            }
            catch (Exception)
            {
                lretval = false;
            }
            return Json(lretval);
        }

        public IActionResult RedirectOffering(string link, string key)
        {
            return Redirect(link);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SupportRepDashboard()
        {
            if (!ISSUPPORTREPRESENTATIVE) return RedirectToAction("Logout", "Login");
            List<tblNotification> notifications = new List<tblNotification>();
            int totalAssignedToMe = 0, total = 0, TotalCloseByMe = 0, unassigned = 0;
            oData.GetSupportRepDashboard(USERID, "tSupportRepDashboard");
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
            oData.GetNotificationForSupport(USERID, "tNotifications");
            if (this.LocalDS.Tables["tNotifications"] != null && this.LocalDS.Tables["tNotifications"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tNotifications"].Rows)
                {
                    notifications.Add(new tblNotification
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        Subject = dr["subject"].ToString().Trim(),
                        Message = dr["message"].ToString().Trim(),
                    });
                }
            }
            string notify = JsonConvert.SerializeObject(notifications);
            HttpContext.Session.SetString("Notification", notify);
            ViewBag.Unassigned = unassigned;
            ViewBag.TotalAssignedToMe = totalAssignedToMe;
            ViewBag.TotalCloseByMe = TotalCloseByMe;
            ViewBag.Total = total;
            ViewData["PageId"] = "OA001";
            return View();
        }

        public IActionResult SupportManagerDashboard()
        {
            if (!ISSUPPORTMANAGER) return RedirectToAction("Logout", "Login");
            int totalAssignedToMe = 0, totalPending = 0, TotalCloseByMe = 0, AssignedtoOther = 0;
            List<tblNotification> notifications = new List<tblNotification>();
            oData.GetNotificationForSupport(USERID, "tNotification");

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
            oData.GetSupportManagerDashboard(USERID, "tSupportManagerDashboard");
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
            return View();
        }

        public JsonResult UpdateCart(int cId, int tLicencse)
        {
            List<tblCart> _carts = new List<tblCart>();
            var response = new Response();
            response.success = false;
            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var success = oData.UpdateCart(cId, tLicencse);
                    if (success)
                    {
                        response.success = true;
                        transaction.Complete();
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    response.msg = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    response.msg = e.Message;
                    transaction.Dispose();
                }
            }
            return Json(response);
        }

        public JsonResult RemoveProduct(int cId)
        {
            var response = new Response();
            response.success = false;
            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var success = oData.RemoveProductsFromCart(cId);
                    if (success)
                    {
                        response.success = true;
                        transaction.Complete();
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    response.msg = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    response.msg = e.Message;
                    transaction.Dispose();
                }
            }
            return Json(response);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.Notification;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KockpitPortal.Controllers.PlanManagement
{
    public class PlanRequestController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public PlanRequestController(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
            _oData = oData;
            _LocalDS = LocalDS;
        }

        public IActionResult Index(int? id)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            if (id != null)
            {
                string notificationData = HttpContext.Session.GetString("Notification");
                if (notificationData != "[]")
                {
                    var res = oData.UpdateNotification(id);
                    if (res)
                    {
                        List<tblNotification> notifications = JsonConvert.DeserializeObject<List<tblNotification>>(notificationData);
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
            List<tblPlanRequest> planRequests = new List<tblPlanRequest>();
            oData.PlanPurchaseRequestGetAll("tPlanRequested");
            if(this.LocalDS.Tables["tPlanRequested"] != null && this.LocalDS.Tables["tPlanRequested"].Rows.Count > 0)
            {
                planRequests = this.LocalDS.Tables["tPlanRequested"].AsEnumerable().Select(row => new tblPlanRequest {
                    ClientId = row.Field<int>("ClientId"),
                    CompanyName = row.Field<string>("CompanyName"),
                    Logo = row.Field<string>("Logo"),
                    EmailId = row.Field<string>("EmailId"),

                    Id = row.Field<int>("Id"),
                    PurchaseDate = row.Field<DateTime>("PurchaseDate"),
                    PaymentMethod = row.Field<string>("PaymentMethod"),
                    PaymentMode = row.Field<string>("PaymentMode"),
                    PaymentTransactionNo = row.Field<string>("PaymentTransactionNo"),
                    IsApproved = row.Field<bool?>("IsApproved"),
                    Remarks = row.Field<string>("Remarks"),
                    CreatedOn = row.Field<DateTime>("CreatedOn"),
                    ActiveStatus = row.Field<bool>("ActiveStatus"),
                }).ToList();
            }

            Dictionary<string, List<tblPlanRequest>> data = planRequests
                            .GroupBy(o => o.CompanyName)
                            .ToDictionary(g => g.Key, g => g.ToList());

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            ViewData["PageId"] = "SA009";
            return View(data);
        }

        [HttpPost]
        public IActionResult Index(int nReqId, string sRemarks, bool lApprove)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            var success = false;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.PlanPurchaseRequestGetById(nReqId, "tPlanPurchaseRequestGetById");
                    if(this.LocalDS.Tables["tPlanPurchaseRequestGetById"] != null 
                        && this.LocalDS.Tables["tPlanPurchaseRequestGetById"].Rows.Count > 0)
                    {
                        var planRequest = new tblPlanRequest { 
                            Id = nReqId,
                            IsApproved = lApprove,
                            Remarks = sRemarks
                        };
                        var res = oData.PlanPurchaseRequestUpdate(planRequest);
                        if (res)
                        {
                            if (lApprove)
                            {
                                //code to insert into main table

                                oData.GetNoOfRequestAgainstRequestId(nReqId, "tNoOfRequest");
                                if(this.LocalDS.Tables["tNoOfRequest"] != null && 
                                    this.LocalDS.Tables["tNoOfRequest"].Rows.Count > 0)
                                {
                                    foreach(DataRow dr in this.LocalDS.Tables["tNoOfRequest"].Rows)
                                    {
                                        var nRequestDetailId = Convert.ToInt32(dr["id"].ToString());
                                        var planPurchaseMaster = this.LocalDS.Tables["tPlanPurchaseRequestGetById"].AsEnumerable().Select(row => new tblPlanPurchase
                                        {
                                            SubscriptionId = Guid.NewGuid().ToString(),
                                            ClientId = row.Field<int>("ClientId"),
                                            PurchaseDate = row.Field<DateTime>("PurchaseDate"),
                                            PaymentMethod = row.Field<string>("PaymentMethod"),
                                            PaymentMode = row.Field<string>("PaymentMode"),
                                            PaymentTransactionNo = row.Field<string>("PaymentTransactionNo"),
                                            CreatedOn = row.Field<DateTime>("CreatedOn"),
                                            ActiveStatus = row.Field<bool>("ActiveStatus"),
                                        }).FirstOrDefault();

                                        var planPurchaseDetails = new List<tblPlanPurchaseDetail>();
                                        oData.PlanPurchaseRequestDetailByRequestDetailId(nRequestDetailId, "tPlanPurchaseRequestDetailByRequestId");
                                        if (this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"] != null
                                            && this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"].Rows.Count > 0)
                                        {
                                            var planRequestDetails = this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"].AsEnumerable().Select(row => new tblPlanRequestDetail
                                            {
                                                PlanId = row.Field<int>("PlanId"),
                                                NoOfUsers = row.Field<int>("NoOfUsers"),
                                                IsWarranty = row.Field<bool>("iswarranty"),
                                                WarrantyInDays = row.Field<int?>("warrantyindays") == null ? 0.0 : Convert.ToDouble(row.Field<int>("warrantyindays"))
                                            }).ToList();

                                            foreach (var item in planRequestDetails)
                                            {
                                                for (int i = 0; i < item.NoOfUsers; i++)
                                                {
                                                    DateTime endDate = DateTime.Today;
                                                    DateTime startDate = new DateTime();
                                                    if (item.IsWarranty)
                                                    {
                                                        var date = DateTime.Today.AddDays(item.WarrantyInDays);
                                                        endDate = date;
                                                        startDate = DateTime.Now;
                                                    }
                                                    else
                                                    {
                                                        endDate = DateTime.MinValue;
                                                        startDate = DateTime.MinValue;
                                                    }

                                                    planPurchaseDetails.Add(new tblPlanPurchaseDetail
                                                    {
                                                        PlanId = item.PlanId,
                                                        LicenseKey = Guid.NewGuid().ToString(),
                                                        CreatedOn = DateTime.Now,
                                                        IsWarranty = item.IsWarranty,
                                                        WarrantyStartDate = startDate,
                                                        WarrantyEndDate = endDate
                                                    });
                                                }
                                            }

                                            success = oData.PlanPurchaseInsert(planPurchaseMaster, planPurchaseDetails);
                                        }
                                    }
                                    if (success)
                                    {
                                        TempData["success"] = "Approved & License Assigned successfully.";
                                        transaction.Complete();
                                        transaction.Dispose();
                                    }
                                }
                            }
                            else
                            {
                                TempData["success"] = "Request Rejected successfully.";
                                transaction.Complete();
                                transaction.Dispose();
                            }
                        }
                    }
                }
                catch (TransactionException ex)
                {
                    TempData["error"] = $"Error : {ex.Message}";
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Error : {ex.Message}";
                    transaction.Dispose();
                }
            }
            return RedirectToAction("Index", "PlanRequest");
        }

        public JsonResult GetPlanRequestDetail(int nReqId)
        {
            Response response = new Response();
            response.success = true;
            List<tblPlanRequestDetail> planRequestDetails = new List<tblPlanRequestDetail>();
            Dictionary<string, List<tblPlanRequestDetail>> data = new Dictionary<string, List<tblPlanRequestDetail>>();
            try
            {
                oData.PlanPurchaseRequestDetailByRequestId(nReqId, "tPlanPurchaseRequestDetailByRequestId");
                if(this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"] != null 
                    && this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"].Rows.Count > 0)
                {
                    planRequestDetails = this.LocalDS.Tables["tPlanPurchaseRequestDetailByRequestId"].AsEnumerable().Select(row => new tblPlanRequestDetail {
                        RequestId = row.Field<int>("RequestId"),
                        PlanId = row.Field<int>("PlanId"),
                        PlanName = row.Field<string>("PlanName"),
                        Description = row.Field<string>("Description"),
                        ValidityDays = row.Field<int>("ValidityDays"),
                        PlanPrice = row.Field<string>("PlanPrice"),
                        NoOfUsers = row.Field<int>("NoOfUsers"),
                        ProjectName = row.Field<string>("ProjectName"),
                        ProjectType = row.Field<string>("ProjectType"),
                        ProjectImage = row.Field<string>("ProjectImage"),
                    }).ToList();
                }

                data = planRequestDetails
                            .GroupBy(o => o.ProjectName)
                            .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                response.success = false;
                response.msg = $"Error : {ex.Message}";
            }
            return Json(new Tuple<Response, Dictionary<string, List<tblPlanRequestDetail>>>(response, data));
        }
    }
}

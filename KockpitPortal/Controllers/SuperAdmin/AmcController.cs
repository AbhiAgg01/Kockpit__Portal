using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class AmcController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public AmcController(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;

            _oData = oData;
            _LocalDS = LocalDS;
        }
        public IActionResult Index()
        {
            List<MyPlans> plans = new List<MyPlans>();
            oData.GetAllPlansPurchasedByClients("tGetPlans");
            oData.GetAllServicePurchasedByClients("tGetService");
            if (this.LocalDS.Tables["tGetPlans"] != null && this.LocalDS.Tables["tGetPlans"].Rows.Count > 0)
            {
                plans = this.LocalDS.Tables["tGetPlans"].AsEnumerable().Select(row => new MyPlans
                {
                    SubscriptionId = row.Field<string>("SubscriptionId"),
                    CompanyName = row.Field<string>("CompanyName"),
                    PlanName = row.Field<string>("planname"),
                    PurchaseDate = row.Field<DateTime>("PurchaseDate"),
                    ClientId = row.Field<int>("clientid"),
                    Logo = row.Field<string>("logo"),
                    PlanId = row.Field<int>("planid"),
                    ProjectId = row.Field<int>("ProjectId"),
                    Iswarranty = row.Field<bool>("iswarranty"),
                    WarrantyEndDate = row.Field<DateTime?>("warrantyenddate"),
                    TotalKeys = Convert.ToInt32(row["TotalKeys"].ToString()),
                    UsedKeys = Convert.ToInt32(row["UsedKeys"].ToString()),
                    UnusedKeys = Convert.ToInt32(row["UnusedKeys"].ToString()),
                    ExpiredKeys = Convert.ToInt32(row["ExpiredKeys"].ToString()),
                    TransferedKeys = Convert.ToInt32(row["TransferedKeys"].ToString()),
                    ISAMCcancelled = row.Field<bool?>("AMCCancelled"),
                    IsAMCExpired = row.Field<bool?>("AMCExpired"),
                    IsAMCReissue = row.Field<bool?>("reissue"),
                    AMCId = string.IsNullOrEmpty(row.Field<int?>("amcid").ToString())? 0 : row.Field<int?>("amcid"),
                    RemainingDays= string.IsNullOrEmpty(row.Field<int?>("remainingdays").ToString()) || row.Field<int?>("remainingdays") < 0 ? 0 : row.Field<int?>("remainingdays"),
                    AMCStartDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcstartdate").ToString())? DateTime.MinValue : row.Field<DateTime?>("amcstartdate"),
                    AMCEndDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcenddate").ToString())? DateTime.MinValue : row.Field<DateTime?>("amcenddate")
                }).ToList();
            }
            if(this.LocalDS.Tables["tGetService"]!=null && this.LocalDS.Tables["tGetService"].Rows.Count > 0)
            {
                var ServiceList= this.LocalDS.Tables["tGetService"].AsEnumerable().Select(row => new MyPlans
                {
                    SubscriptionId = row.Field<string>("SubscriptionId"),
                    CompanyName = row.Field<string>("CompanyName"),
                    PlanName = row.Field<string>("planname"),
                    PurchaseDate = string.IsNullOrEmpty(row.Field<DateTime>("amcstartdate").ToString()) ? DateTime.MinValue : row.Field<DateTime>("amcstartdate"),
                    ClientId = row.Field<int>("clientid"),
                    Logo = row.Field<string>("logo"),
                    PlanId = row.Field<int>("planid"),
                    ProjectId = row.Field<int>("ProjectId"),
                    Iswarranty = false,
                    WarrantyEndDate = DateTime.MinValue,
                    TotalKeys = Convert.ToInt32(row["TotalKeys"].ToString()),
                    UsedKeys = Convert.ToInt32(row["UsedKeys"].ToString()),
                    UnusedKeys = Convert.ToInt32(row["UnusedKeys"].ToString()),
                    ExpiredKeys = Convert.ToInt32(row["ExpiredKeys"].ToString()),
                    TransferedKeys = Convert.ToInt32(row["TransferedKeys"].ToString()),
                    ISAMCcancelled = row.Field<bool?>("AMCCancelled"),
                    IsAMCExpired = row.Field<bool?>("AMCExpired"),
                    IsAMCReissue = row.Field<bool?>("reissue"),
                    AMCId = string.IsNullOrEmpty(row.Field<int?>("amcid").ToString()) ? 0 : row.Field<int?>("amcid"),
                    RemainingDays = string.IsNullOrEmpty(row.Field<int?>("remainingdays").ToString()) || row.Field<int?>("remainingdays") < 0 ? 0 : row.Field<int?>("remainingdays"),
                    AMCStartDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcstartdate").ToString()) ? DateTime.MinValue : row.Field<DateTime?>("amcstartdate"),
                    AMCEndDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcenddate").ToString()) ? DateTime.MinValue : row.Field<DateTime?>("amcenddate")
                }).ToList();

                if(ServiceList!=null && ServiceList.Count > 0)
                {
                    //plans = ServiceList;
                    ServiceList.ForEach(item => plans.Add(item));
                }
            }
            tblAMCViewModel vAMCViewModel = new tblAMCViewModel();
            plans = plans.OrderBy(x => x.ClientId).ToList();
            vAMCViewModel.plans = plans;
            ViewBag.Users = GetAMCClient();
            ViewBag.Projects = ProjectList();
            ViewBag.Service = ServiceList();


            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageTitle"] = "AMC";
            ViewData["PageId"] = "SA010";
            return View(vAMCViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(tblAMCViewModel viewModel)
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    if(viewModel.amc.Id == 0)
                    {
                        oData.UpdateAMC(viewModel.amc.SubscriptionId, viewModel.amc.PlanId, viewModel.amc.ClientId);
                    }
                    oData.AMCUpsert(viewModel.amc);
                    if (viewModel.amc.Id != 0 && ! viewModel.amc.IsCancelled)
                        TempData["success"] = "update amc successfully";
                    else if(viewModel.amc.Id != 0 && viewModel.amc.IsCancelled)
                        TempData["success"] = "cancelled amc succesfully";
                    else
                        TempData["success"] = "amc saved successfully";
                    transaction.Complete();
                    transaction.Dispose();
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
            return RedirectToAction("Index");
        }

        public JsonResult GetDetailsOfAMC(int ClientId, int planId, string subscriptionId,int amcId)
        {
            Response response = new Response();
            tblAMC amc = new tblAMC();
            response.success = false;
            try
            {
                oData.GetAMCPurchasedByClient(ClientId, planId, subscriptionId,amcId, "tAMCClient");
                if (this.LocalDS.Tables["tAMCClient"] != null && this.LocalDS.Tables["tAMCClient"].Rows.Count > 0)
                {
                    amc = this.LocalDS.Tables["tAMCClient"].AsEnumerable().Select(row => new tblAMC
                    {
                        Id = row.Field<int>("id"),
                        ClientId = row.Field<int>("clientid"),
                        PlanId = row.Field<int>("planid"),
                        SubscriptionId = row.Field<string>("subscriptionid"),
                        AMCStartDate = row.Field<DateTime>("amcstartdate"),
                        AMCEndDate = row.Field<DateTime>("amcenddate"),
                        BudgetedManDays = row.Field<string>("budgetedmandays"),
                        ActiveStatus = row.Field<bool>("activestatus"),
                        IsCancelled = row.Field<bool>("iscancelled"),
                    }).FirstOrDefault();
                    response.success = true;
                }
                else
                {
                    response.msg = "no records found...!!!";
                }
            }
            catch (Exception e)
            {
                response.msg = e.Message;
            }
            return Json(new Tuple<Response, tblAMC>(response, amc));
        }

        public JsonResult GetPlansById(int id)
        {
            var response = new Response();
            response.success = false;
            List<tblPlan> tblPlans = new List<tblPlan>();
            try
            {
                oData.PlanGetAllByProject(id, "tPlanGetAll");
                if (this.LocalDS.Tables["tPlanGetAll"] != null
                    && this.LocalDS.Tables["tPlanGetAll"].Rows.Count > 0)
                {
                    tblPlans = this.LocalDS.Tables["tPlanGetAll"].AsEnumerable().Select(row =>
                    new tblPlan
                    {
                        Id = row.Field<int>("Id"),
                        PlanName = row.Field<string>("PlanName"),
                        Description = row.Field<string>("Description"),
                        ProjectId = row.Field<int>("ProjectId"),
                        ProjectName = row.Field<string>("ProjectName"),
                        NoOfUsers = row.Field<int>("NoOfUsers"),
                        ValidityDays = row.Field<int>("ValidityDays"),
                        IsFree = row.Field<bool>("IsFree"),
                        PlanPrice = row.Field<string>("PlanPrice"),
                    }).ToList();
                    response.success = true;
                }
            }
            catch (Exception e)
            {
                response.msg = e.Message;
            }
            return Json(new Tuple<Response, List<tblPlan>>(response, tblPlans));
        }

        public IActionResult AssignAMC(int clientId, int PlanId,DateTime startDate, DateTime endDate, string BudgetedMandays, bool activeStatus, int validity, int projectId,int serviceId)
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {

                    oData.PlanPurchaseCheckLicenseForUser(clientId, projectId, "tAlreadyPlanExists");
                    if (this.LocalDS.Tables["tAlreadyPlanExists"] != null && this.LocalDS.Tables["tAlreadyPlanExists"].Rows.Count > 0)
                    {
                        TempData["error"] = "Selected User has already a active license key for this offering";
                        transaction.Dispose();
                    }
                    else
                    {
                        var planPurchaseMaster = new tblPlanPurchase();
                        var planPurchaseDetails = new List<tblPlanPurchaseDetail>();
                        var amc = new tblAMC();
                        var subId = Guid.NewGuid().ToString();
                        //Offerings
                        if (projectId > 0)
                        {
                            planPurchaseMaster = (new tblPlanPurchase
                            {
                                SubscriptionId = subId,
                                ClientId = clientId,
                                PurchaseDate = DateTime.Now,
                                PaymentMethod = "[Default]",
                                PaymentMode = "[Default]",
                                PaymentTransactionNo = "[Default]",
                                CreatedOn = DateTime.Now,
                                ActiveStatus = true,
                            });

                            planPurchaseDetails.Add(new tblPlanPurchaseDetail
                            {
                                PlanId = PlanId,
                                UserId = clientId,
                                LicenseKey = Guid.NewGuid().ToString(),
                                CreatedOn = DateTime.Now,
                                IsExpired = false,
                                WarrantyStartDate = DateTime.MinValue,
                                WarrantyEndDate = DateTime.MinValue,
                                ExpiryDate = DateTime.Now.AddDays(validity),
                            });
                            var success = oData.ManualPlanPurchaseInsert(planPurchaseMaster, planPurchaseDetails);
                            if (success)
                            {

                                amc = new tblAMC
                                {
                                    PlanId = PlanId,
                                    ClientId = clientId,
                                    AMCEndDate = endDate,
                                    AMCStartDate = startDate,
                                    SubscriptionId = subId,
                                    BudgetedManDays = BudgetedMandays,
                                    ActiveStatus = activeStatus,
                                    Id = 0,
                                    IsService = serviceId,
                                };
                                if (oData.AMCUpsert(amc))
                                {
                                    TempData["success"] = "successfully assign amc to the client";
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                                else
                                {
                                    TempData["error"] = "can not assign amc please try again later";
                                    transaction.Dispose();
                                }
                            }
                            else
                            {
                                TempData["error"] = "can not assign amc please try again later";
                                transaction.Dispose();
                            }
                        }
                        else if (serviceId > 0) //Service
                        {
                            amc = new tblAMC
                            {
                                PlanId = PlanId,
                                ClientId = clientId,
                                AMCEndDate = endDate,
                                AMCStartDate = startDate,
                                SubscriptionId = "",
                                BudgetedManDays = BudgetedMandays,
                                ActiveStatus = activeStatus,
                                Id = 0,
                                IsService = serviceId,
                            };
                            if (oData.AMCUpsert(amc))
                            {
                                TempData["success"] = "successfully assign amc to the client";
                                transaction.Complete();
                                transaction.Dispose();
                            }
                            else
                            {
                                TempData["error"] = "can not assign amc please try again later";
                                transaction.Dispose();
                            }
                        }
                        else
                        {
                            TempData["error"] = "can not assign amc please try again later";
                            transaction.Dispose();
                        }
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
            return RedirectToAction("Index");
        }
    }
}

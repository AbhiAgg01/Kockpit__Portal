using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using Google.Cloud.Firestore;
using KockpitPortal.API;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.Cart;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.Support;
using KockpitPortal.Utility;
using KockpitPortal.ViewModels.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Response = KockpitPortal.Models.Response;

namespace KockpitPortal.Controllers.PlanManagement
{
    public class PlanInfoController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        private ApiManager apiManager;
        GetCurrentDateTime currentDateTime;
        private readonly FirestoreProvider _firestoreProvider;
        System.Threading.CancellationToken cancellationToken;


        public PlanInfoController(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            currentDateTime = new GetCurrentDateTime(config);
            _env = env;
            _oData = oData;
            _LocalDS = LocalDS;
            cancellationToken = new System.Threading.CancellationToken();

            var objFireBase = new Firebase
            {
                type = _config["Firebase:type"].ToString().Trim(),
                project_id = _config["Firebase:project_id"].ToString().Trim(),
                private_key_id = _config["Firebase:private_key_id"].ToString().Trim(),
                private_key = _config["Firebase:private_key"].ToString().Trim(),
                client_email = _config["Firebase:client_email"].ToString().Trim(),
                client_id = _config["Firebase:client_id"].ToString().Trim(),
                auth_uri = _config["Firebase:auth_uri"].ToString().Trim(),
                token_uri = _config["Firebase:token_uri"].ToString().Trim(),
                auth_provider_x509_cert_url = _config["Firebase:auth_provider_x509_cert_url"].ToString().Trim(),
                client_x509_cert_url = _config["Firebase:client_x509_cert_url"].ToString().Trim(),
            };

            _firestoreProvider = new FirestoreProvider(
               new FirestoreDbBuilder
               {
                   ProjectId = objFireBase.project_id,
                   JsonCredentials = JsonConvert.SerializeObject(objFireBase)
               }.Build()
           );
        }

        public IActionResult Index(string planIds = null)
        {
            if (ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            else if (ISSUBADMIN)
            {
                return PartialView("~/Views/PlanInfo/_BuyPlanInstruction.cshtml");
            }
            else
            {
                List<tblCart> _carts = new List<tblCart>();
                oData.GetProductsFromCart(USERID, "tCart");
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
                ViewData["success"] = TempData["success"];
                ViewData["error"] = TempData["error"];
                ViewBag.CartPlans = planIds;
                var list = ProjectList();
                list.RemoveAt(0);


                ViewBag.ProjectList = list;

                ViewData["PageId"] = "OA003";
                ViewData["PageTitle"] = "Buy Plans";
                return View();
            }
        }


        [HttpPost]
        public IActionResult Index(string oMaster, string oDetail)
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    tblPlanRequest planRequest = JsonConvert.DeserializeObject<tblPlanRequest>(oMaster);
                    List<tblPlanRequestDetail> planRequests = JsonConvert.DeserializeObject<List<tblPlanRequestDetail>>(oDetail);
                    if (planRequest != null)
                    {
                        planRequest.PurchaseDate = DateTime.Now;
                        planRequest.ActiveStatus = true;
                        planRequest.ClientId = USERID;
                        planRequest.CreatedOn = DateTime.Now;
                        planRequest.Remarks = string.Empty;

                        oData.PlanPurchaseRequestInsert(planRequest, planRequests);
                        oData.RemoveAllProductsForUsers(USERID);
                        oData.GetSuperAdminDetails("tSuperAdminDetails");
                        if (this.LocalDS.Tables["tSuperAdminDetails"] != null && this.LocalDS.Tables["tSuperAdminDetails"].Rows.Count > 0)
                        {
                           oData.InsertNotification("License Request",("New License Request from client " +COMPANYNAME ), "MESSAGE", currentDateTime.CurrentDatetime().Result ,Convert.ToInt32(this.LocalDS.Tables["tSuperAdminDetails"].Rows[0]["id"].ToString()));
                        }
                        TempData["success"] = "Plan Requested successfully, Please wait for request approval.";
                        transaction.Complete();
                        transaction.Dispose();
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
            return RedirectToAction("RequestedPlan", "PlanInfo");
        }

        public JsonResult ShowPlans(int id)
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

        public IActionResult AddToCart(int offeringId, int planId, int price)
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.CheckDuplicateEntryCart(planId, offeringId, USERID, "tCheckDuplicacy");
                    if (this.LocalDS.Tables["tCheckDuplicacy"] != null && this.LocalDS.Tables["tCheckDuplicacy"].Rows.Count > 0)
                    {
                        TempData["error"] = "product already added to cart";
                        transaction.Dispose();
                    }
                    else
                    {
                        var success = oData.AddProductToCart(USERID, offeringId, planId, price);
                        if (success)
                        {
                            TempData["success"] = "product succesfully added to the cart";
                            transaction.Complete();
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
        public IActionResult CheckOut()
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            List<tblCart> _carts = new List<tblCart>();
            oData.GetProductsFromCart(USERID, "tCart");
            if (this.LocalDS.Tables["tCart"] != null && this.LocalDS.Tables["tCart"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tCart"].Rows)
                {
                    _carts.Add(new tblCart
                    {
                        Id = Convert.ToInt32(dr["id"].ToString().Trim()),
                        OfferingName = dr["projectname"].ToString().Trim(),
                        PlanId = Convert.ToInt32(dr["planid"].ToString().Trim()),
                        PlanName = dr["planname"].ToString().Trim(),
                        Price = Convert.ToInt32(dr["price"].ToString()),
                        TotalLicense = Convert.ToInt32(dr["totallicense"].ToString()),
                    });
                }
            }

            ViewBag.Products = _carts;
            return View();
        }

        public IActionResult ActivePlan()
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");
            List<tblCart> _carts = new List<tblCart>();
            List<MyPlans> plans = new List<MyPlans>();
            oData.PlanPurchaseGetAll("tPlanPurchaseGetAll", USERID);
            if (this.LocalDS.Tables["tPlanPurchaseGetAll"] != null && this.LocalDS.Tables["tPlanPurchaseGetAll"].Rows.Count > 0)
            {
                plans = this.LocalDS.Tables["tPlanPurchaseGetAll"].AsEnumerable().Select(row => new MyPlans
                {
                    SubscriptionId = row.Field<string>("SubscriptionId"),
                    PurchaseDate = row.Field<DateTime>("PurchaseDate"),
                    ProjectId = row.Field<int>("ProjectId"),
                    ProjectName = row.Field<string>("ProjectName"),
                    TotalKeys = Convert.ToInt32(row["TotalKeys"].ToString()),
                    UsedKeys = Convert.ToInt32(row["UsedKeys"].ToString()),
                    UnusedKeys = Convert.ToInt32(row["UnusedKeys"].ToString()),
                    ExpiredKeys = Convert.ToInt32(row["ExpiredKeys"].ToString()),
                    TransferedKeys = Convert.ToInt32(row["TransferedKeys"].ToString()),
                    Id = row.Field<int>("Id"),
                    LicenseKey = row.Field<string>("LicenseKey"),
                    ValidityDays = row.Field<int>("ValidityDays"),
                    AssignTo = row.Field<string>("AssignTo"),
                    ActivationDate = row.Field<DateTime?>("ActivationDate"),
                    ExpiryDate = row.Field<DateTime?>("ExpiryDate"),
                    IsExpired = row.Field<bool?>("IsExpired"),
                    IsTransfered = row.Field<bool>("istransfered"),
                    AMCId = string.IsNullOrEmpty(row.Field<int?>("amcid").ToString()) ? 0 : row.Field<int?>("amcid"),
                    RemainingDays = string.IsNullOrEmpty(row.Field<int?>("remainingdays").ToString()) || row.Field<int?>("remainingdays") < 0 ? 0 : row.Field<int?>("remainingdays"),
                    AMCStartDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcstartdate").ToString()) ? DateTime.MinValue : row.Field<DateTime?>("amcstartdate"),
                    AMCEndDate = string.IsNullOrEmpty(row.Field<DateTime?>("amcenddate").ToString()) ? DateTime.MinValue : row.Field<DateTime?>("amcenddate")
                }).ToList();
            }
            Dictionary<string, List<MyPlans>> data = plans
                            .GroupBy(o => o.ProjectName)
                            .ToDictionary(g => g.Key, g => g.ToList());

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewBag.UserList = GetSubUsersForAssignLicense();

            oData.GetProductsFromCart(USERID, "tUpdatedCart");
            if (this.LocalDS.Tables["tUpdatedCart"] != null && this.LocalDS.Tables["tUpdatedCart"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tUpdatedCart"].Rows)
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

            ViewData["PageTitle"] = "My Plans";
            ViewData["PageId"] = "OA005";
            return View(data);
        }

        public IActionResult RequestedPlan()
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            List<tblPlanRequest> planRequests = new List<tblPlanRequest>();
            oData.PlanPurchaseRequestGetAll("tPlanRequested", USERID);
            if (this.LocalDS.Tables["tPlanRequested"] != null && this.LocalDS.Tables["tPlanRequested"].Rows.Count > 0)
            {
                planRequests = this.LocalDS.Tables["tPlanRequested"].AsEnumerable().Select(row => new tblPlanRequest
                {
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

            List<tblCart> _carts = new List<tblCart>();
            oData.GetProductsFromCart(USERID, "tCart");
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


            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageId"] = "OA004";
            ViewData["PageTitle"] = "Requested Plans";
            return View(planRequests.AsEnumerable());
        }

        public JsonResult GetPlanInfo(int planid)
        {
            tblPlan plan = new tblPlan();
            Response response = new Response();
            response.success = true;
            try
            {
                oData.PlanGetById(planid.ToString(), "tPlanGetById");
                if (this.LocalDS.Tables["tPlanGetById"] != null && this.LocalDS.Tables["tPlanGetById"].Rows.Count > 0)
                {
                    plan = this.LocalDS.Tables["tPlanGetById"].AsEnumerable().Select(row => new tblPlan
                    {
                        Id = row.Field<int>("Id"),
                        PlanName = row.Field<string>("PlanName"),
                        Description = row.Field<string>("Description"),
                        ProjectId = row.Field<int>("ProjectId"),
                        PlanPrice = row.Field<string>("PlanPrice"),
                    }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.msg = $"Error : {ex.Message}";
                response.success = false;
            }
            return Json(new Tuple<Response, tblPlan>(response, plan));
        }

        public JsonResult AssignLicense(int nId, int nUserId, int nProjectId)
        {
            Response response = new Response();
            response.success = true;

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    //check for already has active license key
                    oData.PlanPurchaseCheckLicenseForUser(nUserId, nProjectId, "tAlreadyPlanExists");
                    if (this.LocalDS.Tables["tAlreadyPlanExists"] != null && this.LocalDS.Tables["tAlreadyPlanExists"].Rows.Count > 0)
                    {
                        response.success = false;
                        response.msg = "Selected User has already a active license key for this offering";
                    }
                    else
                    {
                        //get license info
                        oData.PlanPurchaseGetLicenseInfo(nId, "tPlanInfo");
                        if (this.LocalDS.Tables["tPlanInfo"] != null && this.LocalDS.Tables["tPlanInfo"].Rows.Count > 0)
                        {
                            var validity = Convert.ToInt32(this.LocalDS.Tables["tPlanInfo"].Rows[0]["ValidityDays"].ToString());
                            var activationDate = DateTime.Now;
                            var expiryDate = DateTime.Now.AddDays(validity);
                            var planPurcahseDetail = new tblPlanPurchaseDetail
                            {
                                Id = nId,
                                ActivationDate = activationDate,
                                ExpiryDate = expiryDate,
                                IsExpired = false,
                                UserId = nUserId
                            };
                            oData.PlanPurchaseAssignLicense(planPurcahseDetail);

                            var isChat = Convert.ToBoolean(this.LocalDS.Tables["tPlanInfo"].Rows[0]["IsChat"].ToString());
                            if (isChat)
                            {
                                var firebaseResponse = HandleFireBaseUser(nUserId).Result;
                                //TODO : code to check if IsChat then add user in firebase
                                if (firebaseResponse)
                                {
                                    response.msg = "License assigned successfully.";
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                            }
                            else
                            {
                                response.msg = "License assigned successfully.";
                                transaction.Complete();
                                transaction.Dispose();
                            }
                        }
                        else
                        {
                            response.success = false;
                            response.msg = "No Plan Found";
                        }
                    }
                }
                catch (TransactionException ex)
                {
                    response.msg = $"Error : {ex.Message}";
                    response.success = false;
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    response.msg = $"Error : {ex.Message}";
                    response.success = false;
                    transaction.Dispose();
                }
            }

            return Json(response);
        }

        public JsonResult TransferLicense(int nId, int nUserId, int nProjectId)
        {
            Response response = new Response();
            response.success = true;

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    //check for already has active license key
                    oData.PlanPurchaseCheckLicenseForUser(nUserId, nProjectId, "tAlreadyPlanExists");
                    if (this.LocalDS.Tables["tAlreadyPlanExists"] != null && this.LocalDS.Tables["tAlreadyPlanExists"].Rows.Count > 0)
                    {
                        response.success = false;
                        response.msg = "Selected User has already a active license key for this project";
                    }
                    else
                    {
                        //update current license and expire
                        response.success = oData.PlanPurchaseExpireLicense(nId);
                        if (response.success)
                        {
                            //get license info
                            oData.PlanPurchaseGetLicenseInfo(nId, "tPlanInfo");
                            if (this.LocalDS.Tables["tPlanInfo"] != null && this.LocalDS.Tables["tPlanInfo"].Rows.Count > 0)
                            {
                                var planPurcahseDetail = new tblPlanPurchaseDetail
                                {
                                    SubscriptionId = this.LocalDS.Tables["tPlanInfo"].Rows[0]["SubscriptionId"].ToString(),
                                    PlanId = Convert.ToInt32(this.LocalDS.Tables["tPlanInfo"].Rows[0]["PlanId"].ToString()),
                                    LicenseKey = Guid.NewGuid().ToString(),
                                    ActivationDate = DateTime.Now,
                                    ExpiryDate = Convert.ToDateTime(this.LocalDS.Tables["tPlanInfo"].Rows[0]["ExpiryDate"].ToString()),
                                    IsExpired = false,
                                    UserId = nUserId,
                                    TransferFrom = this.LocalDS.Tables["tPlanInfo"].Rows[0]["LicenseKey"].ToString(),
                                    CreatedOn = DateTime.Now,
                                    ActiveStatus = true,
                                    DeviceId = this.LocalDS.Tables["tPlanInfo"].Rows[0]["DeviceId"].ToString(),
                                    IsWarranty = Convert.ToBoolean(this.LocalDS.Tables["tPlanInfo"].Rows[0]["iswarranty"].ToString()),
                                    WarrantyStartDate = Convert.ToDateTime(this.LocalDS.Tables["tPlanInfo"].Rows[0]["warrantystartdate"].ToString()),
                                    WarrantyEndDate = Convert.ToDateTime(this.LocalDS.Tables["tPlanInfo"].Rows[0]["warrantyenddate"].ToString()),
                                    IsTransfered = false
                                };

                                oData.PlanPurchaseTransferLicense(planPurcahseDetail);

                                var isChat = Convert.ToBoolean(this.LocalDS.Tables["tPlanInfo"].Rows[0]["IsChat"].ToString());
                                if (isChat)
                                {
                                    var firebaseResponse = HandleFireBaseUserTransfer(Convert.ToInt32(this.LocalDS.Tables["tPlanInfo"].Rows[0]["UserId"].ToString())).Result;
                                    //TODO : code to check if IsChat then add user in firebase
                                    if (firebaseResponse)
                                    {
                                        firebaseResponse = HandleFireBaseUser(nUserId).Result;
                                        if (firebaseResponse)
                                        {
                                            response.msg = "License transfered successfully.";
                                            transaction.Complete();
                                            transaction.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    response.msg = "License transfered successfully.";
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                            }
                        }
                    }
                }
                catch (TransactionException ex)
                {
                    response.msg = $"Error : {ex.Message}";
                    response.success = false;
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    response.msg = $"Error : {ex.Message}";
                    response.success = false;
                    transaction.Dispose();
                }
            }

            return Json(response);
        }


        public JsonResult GetNewLicenseInfo(int nId)
        {
            tblPlanPurchaseDetail planPurchaseDetail = new tblPlanPurchaseDetail();
            Response response = new Response();
            response.success = true;
            try
            {
                oData.PlanPurchaseGetLicenseInfo(nId, "tPlanInfo");
                if (this.LocalDS.Tables["tPlanInfo"] != null && this.LocalDS.Tables["tPlanInfo"].Rows.Count > 0)
                {
                    var licenseKey = this.LocalDS.Tables["tPlanInfo"].Rows[0]["LicenseKey"].ToString();
                    var validityDays = this.LocalDS.Tables["tPlanInfo"].Rows[0]["ValidityDays"].ToString();
                    var expiryDate = this.LocalDS.Tables["tPlanInfo"].Rows[0]["ExpiryDate"].ToString();

                    planPurchaseDetail = new tblPlanPurchaseDetail
                    {
                        ExpiryDate = Convert.ToDateTime(expiryDate),
                        TransferFrom = licenseKey
                    };
                }
            }
            catch (Exception ex)
            {
                response.msg = $"Error : {ex.Message}";
                response.success = false;
            }
            return Json(new Tuple<Response, tblPlanPurchaseDetail>(response, planPurchaseDetail));
        }


        private async Task<bool> HandleFireBaseUserTransfer(int nUserId)
        {
            bool resFirebaseApi = true;
            oData.ClientUserGetById(nUserId, "tFirebaseUserInfo");
            if (this.LocalDS.Tables["tFirebaseUserInfo"] != null
                && this.LocalDS.Tables["tFirebaseUserInfo"].Rows.Count > 0)
            {

                resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                        this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        "CheckUser");

                if (resFirebaseApi)
                {
                    resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                        this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        "CheckUserActive");

                    if (resFirebaseApi)
                    {
                        var objFireBaseUser = new KockpitPortal.Models.Firebase.Users
                        {
                            CompanyDomain = COMPANYNAME,
                            userId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmpCode"].ToString().Trim(),
                            UserName = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["CompanyName"].ToString().Trim(),
                            eMailId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                            IsActive = false
                        };

                        resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                            this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                            "UpdateUser", objFireBaseUser);
                    }
                }
            }

            return resFirebaseApi;
        }

        private async Task<bool> HandleFireBaseUser(int nUserId)
        {
            bool resFirebaseApi = true;
            oData.ClientUserGetById(nUserId, "tFirebaseUserInfo");
            if (this.LocalDS.Tables["tFirebaseUserInfo"] != null
                && this.LocalDS.Tables["tFirebaseUserInfo"].Rows.Count > 0)
            {
                //check company if not then create
                //TODO

                //check user
                resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                        this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        "CheckUser");

                if (!resFirebaseApi)
                {
                    //code to create new user
                    var objFireBaseUser = new KockpitPortal.Models.Firebase.Users
                    {
                        CompanyDomain = COMPANYNAME,
                        userId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmpCode"].ToString().Trim(),
                        UserName = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["CompanyName"].ToString().Trim(),
                        eMailId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        IsActive = true
                    };

                    resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                        this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        "CreateUser", objFireBaseUser);
                }
                else
                {
                    resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                        this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                        "CheckUserActive");

                    if (!resFirebaseApi)
                    {
                        var objFireBaseUser = new KockpitPortal.Models.Firebase.Users
                        {
                            CompanyDomain = COMPANYNAME,
                            userId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmpCode"].ToString().Trim(),
                            UserName = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["CompanyName"].ToString().Trim(),
                            eMailId = this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                            IsActive = true
                        };

                        resFirebaseApi = await HandleFireBaseUserApi(COMPANYNAME,
                            this.LocalDS.Tables["tFirebaseUserInfo"].Rows[0]["EmailId"].ToString().Trim(),
                            "UpdateUser", objFireBaseUser);
                    }
                }
            }

            return resFirebaseApi;
        }

        private async Task<bool> HandleFireBaseUserApi(string companyName, string emailId, string type, KockpitPortal.Models.Firebase.Users user = null)
        {
            try
            {
                if (type == "CheckUser" || type == "CheckUserActive")
                {
                    var res = await _firestoreProvider.CheckUserByEmail<KockpitPortal.Models.Firebase.Users>($"Organisations/{companyName}/users",
                    $"eMailId", emailId, cancellationToken);
                    if (res != null && res.Count() > 0)
                        return type == "CheckUser" ? true : res.FirstOrDefault().IsActive;
                    else
                        return false;
                }
                else
                {
                    try
                    {
                        if(type == "CreateUser")
                            await _firestoreProvider.Add($"Organisations/{companyName}/users", user, cancellationToken);
                        else if(type == "UpdateUser")
                        {
                            await _firestoreProvider.Update($"Organisations/{companyName}/users",
                                _firestoreProvider.GetKey($"Organisations/{companyName}/users", user.eMailId, cancellationToken).Result,
                                user,
                                cancellationToken);
                        }
                        else if(type == "RemoveUser")
                        {
                            await _firestoreProvider.Remove($"Organisations/{companyName}/users",
                                _firestoreProvider.GetKey($"Organisations/{companyName}/users", user.eMailId, cancellationToken).Result,
                                cancellationToken);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public JsonResult GetOfferingsFaQ(int offeringId)
        {
            var response = new Response();
            response.success = false;
            List<tblFAQ> faqs = new List<tblFAQ>();
            try
            {
                oData.GetFAQByOffering(offeringId, "tFAQ");
                if(this.LocalDS.Tables["tFAQ"] != null && this.LocalDS.Tables["tFAQ"].Rows.Count > 0)
                {
                    faqs = this.LocalDS.Tables["tFAQ"].AsEnumerable().Select(row =>
                    new tblFAQ()
                    {
                        Question = row.Field<string>("Question"),
                        Answer = row.Field<string>("ans")
                    }).ToList();
                    response.success = true;
                }
            }catch(Exception e)
            {
                response.msg = e.Message;
            }
            return Json(new Tuple<Response,List<tblFAQ>>(response,faqs));
        }
    }
}

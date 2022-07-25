using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.Utility;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Service
{
    public class Agent : CronJobService
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _baseConfig;
        private ApiManager apiManager;
        GetCurrentDateTime currentDateTime;

        private readonly ILogger<Agent> _logger;

        public Agent(IScheduleConfig<Agent> config, ILogger<Agent> logger, IConfiguration sconfig, IHostingEnvironment env)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _env = env;
            _logger = logger;
            _baseConfig = sconfig;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            currentDateTime = new GetCurrentDateTime(sconfig);
        }

        public string KockpitAuthenticatorConnection(string strDatabase = "")
        {
            PostgreConnection postgreConnection = new PostgreConnection();
            postgreConnection.Host = _baseConfig["KockpitAuthenticatorPsql:Host"] != null ? _baseConfig["KockpitAuthenticatorPsql:Host"].ToString() : "";
            postgreConnection.Port = _baseConfig["KockpitAuthenticatorPsql:Port"] != null ? _baseConfig["KockpitAuthenticatorPsql:Port"].ToString() : "";
            postgreConnection.Username = _baseConfig["KockpitAuthenticatorPsql:Username"] != null ? _baseConfig["KockpitAuthenticatorPsql:Username"].ToString() : "";
            postgreConnection.Password = _baseConfig["KockpitAuthenticatorPsql:Password"] != null ? _baseConfig["KockpitAuthenticatorPsql:Password"].ToString() : "";
            postgreConnection.Database = string.IsNullOrEmpty(strDatabase)
                ? _baseConfig["KockpitAuthenticatorPsql:Database"] != null ? _baseConfig["KockpitAuthenticatorPsql:Database"].ToString() : ""
                : strDatabase;
            return string.IsNullOrEmpty(postgreConnection.Port)
                ? $"Host={postgreConnection.Host};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}"
                : $"Host={postgreConnection.Host};Port={postgreConnection.Port};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Agent starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Agent is working.");
            UpdateLicense();
            UpdateLicenseWarranty();
            UpdateAMC();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Agent is stopping.");
            return base.StopAsync(cancellationToken);
        }

        private void UpdateLicense()
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    List<tblPlanPurchaseDetail> activeLicenses = new List<tblPlanPurchaseDetail>();
                    // todo: Add repeating code here
                    oData.PlanPurchaseGetAllActiveLicense("tActiveLicense");
                    if (this.LocalDS.Tables["tActiveLicense"] != null && this.LocalDS.Tables["tActiveLicense"].Rows.Count > 0)
                    {
                        activeLicenses = this.LocalDS.Tables["tActiveLicense"].AsEnumerable().Select(row =>
                        new tblPlanPurchaseDetail()
                        {
                            Id = Convert.ToInt32(row["Id"].ToString().Trim()),
                            ExpiryDate = row.Field<DateTime>("ExpiryDate"),
                            LicenseKey = row.Field<string>("licensekey"),
                            UserId = row.Field<int>("userid"),
                            UserEmail = row.Field<string>("emailid"),
                            UserName = row.Field<string>("username"),
                            
                        }).ToList();

                        if (activeLicenses != null && activeLicenses.Count > 0)
                        {
                            var currentDate = DateTime.Now.Date;
                            foreach (var item in activeLicenses)
                            {
                                var licenseExpiryDate = item.ExpiryDate.Date;
                                if (licenseExpiryDate < currentDate)
                                {
                                    //code to update the expiry of license
                                    oData.PlanPurchaseExpireLicense(item.Id);

                                    //TODO : code to check if IsChat then add user in firebase
                                    var company = this.LocalDS.Tables["tActiveLicense"].Rows[0]["CompanyName"].ToString().Trim();
                                    var email = this.LocalDS.Tables["tActiveLicense"].Rows[0]["EmailId"].ToString().Trim();
                                    var username = this.LocalDS.Tables["tActiveLicense"].Rows[0]["Username"].ToString().Trim();
                                    var empCode = this.LocalDS.Tables["tActiveLicense"].Rows[0]["EmpCode"].ToString().Trim();

                                    HandleFireBaseUser(company, email, username, empCode);
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                                else if(licenseExpiryDate < currentDate.AddDays(4))
                                {
                                    //var email = this.LocalDS.Tables["tActiveLicense"].Rows[0]["EmailId"].ToString().Trim();
                                    //var username = this.LocalDS.Tables["tActiveLicense"].Rows[0]["Username"].ToString().Trim();
                                    //var licenseKey = this.LocalDS.Tables["tActiveLicense"].Rows[0]["licensekey"].ToString().Trim();
                                    //var userId = Convert.ToInt32(this.LocalDS.Tables["tActiveLicense"].Rows[0]["userid"].ToString().Trim());
                                    var message = String.Format("Your licence key {0} is expiring soon.",item.LicenseKey);

                                    //check is notification send already for the day
                                    oData.IsNotificationSendToday(item.Id, "tCheckNotificationSend");
                                    if(this.LocalDS.Tables["tCheckNotificationSend"]  == null || this.LocalDS.Tables["tCheckNotificationSend"].Rows.Count == 0)
                                    {
                                        // send Notification
                                        oData.InsertNotification("License Expiring", message, "MESSAGE", currentDateTime.CurrentDatetime().Result,item.UserId);

                                        // send Mail
                                        string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "LicenseExpireNotification.html")))
                                           ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "LicenseExpireNotification.html"))
                                               : string.Empty;

                                        string sMailOutput = "";
                                        string sMailSubject = "License Expiring";
                                        string sMailBody = string.Format(mailBody.ToString(), item.UserName, message);

                                        SendMail(item.UserEmail, sMailSubject, sMailBody, null, out sMailOutput);

                                        // Insert record in notification history
                                        oData.InsertNotificationHistory(item.Id);

                                        transaction.Complete();
                                        transaction.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (TransactionException ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
            }
        }

        private void UpdateLicenseWarranty()
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    List<tblPlanPurchaseDetail> activeLicenses = new List<tblPlanPurchaseDetail>();
                    // todo: Add repeating code here
                    oData.PlanPurchaseGetAllActiveWarranty("tActiveLicense");
                    if (this.LocalDS.Tables["tActiveLicense"] != null && this.LocalDS.Tables["tActiveLicense"].Rows.Count > 0)
                    {
                        activeLicenses = this.LocalDS.Tables["tActiveLicense"].AsEnumerable().Select(row =>
                        new tblPlanPurchaseDetail()
                        {
                            Id = Convert.ToInt32(row["Id"].ToString().Trim()),
                            WarrantyEndDate = row.Field<DateTime>("warrantyenddate")
                        }).ToList();

                        if (activeLicenses != null && activeLicenses.Count > 0)
                        {
                            var currentDate = DateTime.Now.Date;
                            foreach (var item in activeLicenses)
                            {
                                var licenseWarrantyEndDate = item.WarrantyEndDate.Date;
                                if (licenseWarrantyEndDate < currentDate)
                                {
                                    //code to update the expiry of license
                                    oData.PlanPurchaseExpireWarranty(item.Id);
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                            }
                        }
                    }
                }
                catch (TransactionException ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
            }
        }

        private void UpdateAMC()
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    List<tblAMC> amc = new List<tblAMC>();
                    // todo: Add repeating code here
                    oData.GetAllUnExpiredAMC("tAMC");
                    if (this.LocalDS.Tables["tAMC"] != null && this.LocalDS.Tables["tAMC"].Rows.Count > 0)
                    {
                        amc = this.LocalDS.Tables["tAMC"].AsEnumerable().Select(row =>
                        new tblAMC()
                        {
                            Id = Convert.ToInt32(row["Id"].ToString().Trim()),
                            AMCEndDate = row.Field<DateTime>("amcenddate")
                        }).ToList();

                        if (amc != null && amc.Count > 0)
                        {
                            var currentDate = DateTime.Now.Date;
                            foreach (var item in amc)
                            {
                                var amcEndDate = item.AMCEndDate.Date;
                                if (amcEndDate < currentDate)
                                {
                                    //code to update the expiry of license
                                    oData.ExpiredAMC(item.Id);
                                    transaction.Complete();
                                    transaction.Dispose();
                                }
                            }
                        }
                    }
                }
                catch(TransactionException ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error : {ex.Message}");
                    transaction.Dispose();
                }
            }
        }

        private bool HandleFireBaseUser(string company, string email, string username, string empcode)
        {
            bool resFirebaseApi = true;
            resFirebaseApi = HandleFireBaseUserApi(company, email, "CheckUser");
            if (resFirebaseApi)
            {
                resFirebaseApi = HandleFireBaseUserApi(company, email, "CheckUserActive");
                if (resFirebaseApi)
                {
                    var objFireBaseUser = new KockpitPortal.Models.Firebase.Users
                    {
                        CompanyDomain = company,
                        userId = empcode,
                        UserName = username,
                        eMailId = email,
                        IsActive = false
                    };

                    resFirebaseApi = HandleFireBaseUserApi(company, email, "UpdateUser", JsonConvert.SerializeObject(objFireBaseUser));
                }
            }
            return resFirebaseApi;
        }
        private bool HandleFireBaseUserApi(string companyName, string emailId, string type, string obj = null)
        {
            try
            {
                var ApiUrl = FirebaseApi(type);
                switch (type)
                {
                    case "CreateUser":
                    case "UpdateUser":
                    case "RemoveUser":
                        ApiUrl = ApiUrl + $"?company={companyName}";
                        break;
                }
                apiManager = new ApiManager(ApiUrl);
                if (type == "CheckUser" || type == "CheckUserActive")
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("company", companyName);
                    parameters.Add("email", emailId);
                    var res = apiManager.Post(parameters);
                    return (res.Result.Item1 == HttpStatusCode.OK);
                }
                else
                {
                    var res = apiManager.Post(JsonConvert.SerializeObject(obj).ToString().Trim());
                    return (res.Result.Item1 == HttpStatusCode.OK);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string FirebaseApi(string type)
        {
            return _baseConfig[$"FirebaseAPI:{type}"] != null ? _baseConfig[$"FirebaseAPI:{type}"].ToString() : "";
        }

        public bool SendMail(string strReceiver, string strSubject, string strBody, List<string> ccEmails, out string sentMsg)
        {
            try
            {
                string strSenderEmail = _baseConfig["MailCredential:Email"] != null ? _baseConfig["MailCredential:Email"].ToString() : "";
                string strSenderPassword = _baseConfig["MailCredential:Password"] != null ? _baseConfig["MailCredential:Password"].ToString() : "";
                int strSenderPort = _baseConfig["MailCredential:Port"] != null ? Convert.ToInt32(_baseConfig["MailCredential:Port"].ToString()) : 587;
                string strSenderHost = _baseConfig["MailCredential:Host"] != null ? _baseConfig["MailCredential:Host"].ToString() : "";
                bool lEnableSsl = _baseConfig["MailCredential:EnableSsl"] != null ? Convert.ToBoolean(_baseConfig["MailCredential:EnableSsl"].ToString()) : true;

                Mail mail = new Mail(strSenderEmail, strSenderPassword, strSenderPort, strSenderHost, lEnableSsl);
                mail.SendMail(strReceiver, strSubject, strBody, ccEmails, out sentMsg);
                return true;
            }
            catch (Exception ex)
            {
                sentMsg = ex.Message;
                return false;
            }
        }

       

    }
}

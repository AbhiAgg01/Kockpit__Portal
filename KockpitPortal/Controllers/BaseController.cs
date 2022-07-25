using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using KockpitPortal.DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using KockpitPortal.Models;

namespace KockpitPortal.Controllers
{
    public class BaseController : Controller
    {
        CookieOptions option;
        CreateLog _log;
        public BaseController()
        {
            option = new CookieOptions();
            option.Expires = DateTime.Now.AddMinutes(30);
            option.Domain = ".kockpit.in";
            //option.Domain = "localhost";
        }
        public CookieOptions CookieOpt { get { return option; } set { value = option; } }


        public DataPostgres _oData { get; set; }
        public DataSet _LocalDS { get; set; }
        public IConfiguration _baseConfig { get; set; }
        public IHostingEnvironment _baseEnv { get; set; }

        public bool SESSIONEXISTS
        {
            get { return (HttpContext.Session.GetString("SessionInfo_EmailId") == null) ? false : true; }
        }
        public bool ISSUPERADMIN
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Role") == "SUPERADMIN") ? true : false;
            }
        }
        public bool ISADMIN
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Role") == "ADMIN") ? true : false;
            }
        }
        public bool ISSUBADMIN
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Role") == "SUBADMIN") ? true : false;
            }
        }
        public string SCHEMA
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_SchemaName") == null) ? string.Empty
                    : HttpContext.Session.GetString("SessionInfo_SchemaName").Trim();
            }
        }
        public string SOCKETID
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_SocketId") == null) ? string.Empty
                    : HttpContext.Session.GetString("SessionInfo_SocketId").Trim();
            }
        }

        public int USERID
        {
            get
            {
                return (HttpContext.Session.GetInt32("SessionInfo_Id") == null) ? 0
                    : HttpContext.Session.GetInt32("SessionInfo_Id").Value;
            }
        }

        public bool ISSUPPORTMANAGER
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Role") == "SUPPORTMANAGER") ? true : false;
            }
        }

        public bool ISSUPPORTREPRESENTATIVE
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Role") == "SUPPORTREPRESENTATIVE") ? true : false;
            }
        }

        public string WHATSAPPNUMBER
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_ContactNumber") == null) ? ""
                    : HttpContext.Session.GetString("SessionInfo_ContactNumber").ToString();
            }
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!SESSIONEXISTS)
            {
                filterContext.Result = new RedirectResult("~/Login/Logout");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        public string USEREMAIL
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_EmailId") == null) ? ""
                    : HttpContext.Session.GetString("SessionInfo_EmailId").ToString();
            }
        }

        public string USERPWD
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_Password") == null) ? ""
                    : HttpContext.Session.GetString("SessionInfo_Password").ToString();
            }
        }

        public string COMPANYNAME
        {
            get
            {
                return (HttpContext.Session.GetString("SessionInfo_CompanyName") == null) ? ""
                    : HttpContext.Session.GetString("SessionInfo_CompanyName").ToString();
            }
        }

        public string GeneratePassword(int passLength)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz@#$&ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, passLength)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
        public string ConfiguratorConnection(string strDatabase = "")
        {
            PostgreConnection postgreConnection = new PostgreConnection();
            postgreConnection.Host = _baseConfig["ConfiguratorPsql:Host"] != null ? _baseConfig["ConfiguratorPsql:Host"].ToString() : "";
            postgreConnection.Port = _baseConfig["ConfiguratorPsql:Port"] != null ? _baseConfig["ConfiguratorPsql:Port"].ToString() : "";
            postgreConnection.Username = _baseConfig["ConfiguratorPsql:Username"] != null ? _baseConfig["ConfiguratorPsql:Username"].ToString() : "";
            postgreConnection.Password = _baseConfig["ConfiguratorPsql:Password"] != null ? _baseConfig["ConfiguratorPsql:Password"].ToString() : "";
            postgreConnection.Database = string.IsNullOrEmpty(strDatabase)
                ? _baseConfig["ConfiguratorPsql:Database"] != null ? _baseConfig["ConfiguratorPsql:Database"].ToString() : ""
                : strDatabase;
            return string.IsNullOrEmpty(postgreConnection.Port)
                ? $"Host={postgreConnection.Host};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}"
                : $"Host={postgreConnection.Host};Port={postgreConnection.Port};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}";
        }

        public string FirebaseApi(string type)
        {
            return _baseConfig[$"FirebaseAPI:{type}"] != null ? _baseConfig[$"FirebaseAPI:{type}"].ToString() : "";
        }

        public string SUPPORTEMAIL
        {
            get
            {
                return (_baseConfig["MailCredential:SupportEmail"] != null ? _baseConfig["MailCredential:SupportEmail"].ToString() : "");
            }
        }

        class Links
        {
            public string label { get; set; }
            public string the_link { get; set; }
        }

        public JsonResult GetLinks()
        {
            string jsonlinks = string.Empty;
            try
            {
                List<Links> links = new List<Links>();
                if (this.SESSIONEXISTS)
                {
                    if (this.ISSUPERADMIN)
                    {
                        links.Add(new Links { label = "Dashboard", the_link = Url.Action("Index", "Home", null, Request.Scheme) });
                    }
                    else if (this.ISADMIN)
                    {
                        links.Add(new Links { label = "Dashboard", the_link = Url.Action("Dashboard", "Home", null, Request.Scheme) });
                    }
                    else if (this.ISSUBADMIN)
                    {
                        links.Add(new Links { label = "Dashboard", the_link = Url.Action("Dashboard", "Home", null, Request.Scheme) });
                    }
                }

                jsonlinks = JsonConvert.SerializeObject(links, Formatting.Indented);
            }
            catch (Exception)
            {
            }
            return Json(jsonlinks);
        }


        #region [SelectListItems]
        public List<SelectListItem> ProjectList() 
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.ProjectGetAll("tProjectGetAll");
            if(this._LocalDS.Tables["tProjectGetAll"] != null && this._LocalDS.Tables["tProjectGetAll"].Rows.Count > 0)
            {
                foreach(DataRow item in this._LocalDS.Tables["tProjectGetAll"].Rows)
                {
                    selectListItems.Add(new SelectListItem { 
                        Text = item["ProjectName"].ToString().Trim(), 
                        Value = item["Id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> ServiceList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetService("tGetService");
            if (this._LocalDS.Tables["tGetService"] != null && this._LocalDS.Tables["tGetService"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tGetService"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["Servicename"].ToString().Trim(),
                        Value = item["Id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> ProjectCategoryList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.ProjectCategoryAll("tProjectCategoryAll");
            if (this._LocalDS.Tables["tProjectCategoryAll"] != null && this._LocalDS.Tables["tProjectCategoryAll"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tProjectCategoryAll"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["OfferingCategory"].ToString().Trim(),
                        Value = item["OfferingCategory"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> FormCategoryList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.FormCategoryGetAll("tFormCategoryGetAll");
            if (this._LocalDS.Tables["tFormCategoryGetAll"] != null && this._LocalDS.Tables["tFormCategoryGetAll"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tFormCategoryGetAll"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["CategoryName"].ToString().Trim(),
                        Value = item["Id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }


        public List<SelectListItem> ModuleList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.ModuleGetAll("tModuleGetAll");
            if (this._LocalDS.Tables["tModuleGetAll"] != null 
                && this._LocalDS.Tables["tModuleGetAll"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tModuleGetAll"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["ModuleName"].ToString().Trim(),
                        Value = item["Id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> UserList(string UserType, int ParentId = 0)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });

            switch (UserType)
            {
                case "CLIENT":
                    _oData.ClientUserGetAll(ParentId, "tUserList");
                    break;
                case "EMPLOYEE":
                    _oData.EmployeeUserGetAll(ParentId, "tUserList");
                    break;
                case "SUBADMIN":
                    _oData.SubUserGetAll(ParentId, "tUserList");
                    break;
            }

            if (this._LocalDS.Tables["tUserList"] != null
                && this._LocalDS.Tables["tUserList"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tUserList"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["CompanyName"].ToString().Trim(),
                        Value = item["Id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> GetEmpoloyeeList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            selectListItems.Add(new SelectListItem { Text = "Self Assign", Value = USERID.ToString() });
            _oData.GetAllEmployee("tEmployeeAll");
            if (this._LocalDS.Tables["tEmployeeAll"] != null && this._LocalDS.Tables["tEmployeeAll"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tEmployeeAll"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Text = item["companyname"].ToString().Trim(),
                        Value = item["id"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> GetPlans()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetPlans(USERID, "tLicense");
            _oData.GetIssueService(USERID, "tIssueService");
            List<PlanData> pd = new List<PlanData>();
            if (this._LocalDS.Tables["tLicense"] != null && this._LocalDS.Tables["tLicense"].Rows.Count > 0)
            {
                 pd = (from a in this._LocalDS.Tables["tLicense"].AsEnumerable()
                           select new PlanData
                           {
                               subscriptionid = a.Field<string>("subscriptionid"),
                               projectname = a.Field<string>("projectname")
                           }).ToList();
                
                //foreach (DataRow item in this._LocalDS.Tables["tLicense"].Rows)
                //{
                //    selectListItems.Add(new SelectListItem
                //    {
                //        Value = item["subscriptionid"].ToString().Trim(),
                //        Text = item["projectname"].ToString().Trim()
                //    });
                //}
            }
            if (this._LocalDS.Tables["tIssueService"] != null && this._LocalDS.Tables["tIssueService"].Rows.Count > 0)
            {
                var service = (from a in this._LocalDS.Tables["tIssueService"].AsEnumerable()
                               select new PlanData
                               {
                                   subscriptionid = a.Field<int>("id").ToString() + "-Service",
                                   projectname = a.Field<string>("servicename")
                               }).ToList();
                if (service != null && service.Count > 0)
                {
                    service.ForEach(item => pd.Add(item));
                }
            }
            if (pd != null && pd.Count > 0)
            {
                pd.ForEach(item => selectListItems.Add(new SelectListItem { Value = item.subscriptionid, Text = item.projectname }));
            }
            return selectListItems;
        }

        public class PlanData
        {
            public string subscriptionid { get; set; }
            public string projectname { get; set; }
        }

        public List<SelectListItem> GetSubUsersForAssignLicense()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetSubUsersWithEmail(USERID, "tLicense");
            if (this._LocalDS.Tables["tLicense"] != null && this._LocalDS.Tables["tLicense"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tLicense"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Value = item["id"].ToString().Trim(),
                        Text = item["CompanyName"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }


        public List<SelectListItem> GetIssueType()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetIssueType("tIssuetype");
            if (this._LocalDS.Tables["tIssuetype"] != null && this._LocalDS.Tables["tIssuetype"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tIssuetype"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Value = item["id"].ToString().Trim(),
                        Text = item["issuetype"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }

        public List<SelectListItem> GetPlansSupport()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetPlansForSupport("tLicense");
            //_oData.GetServiceForSupport("tIssueService");
            List<PlanData> pd = new List<PlanData>();
            if (this._LocalDS.Tables["tLicense"] != null && this._LocalDS.Tables["tLicense"].Rows.Count > 0)
            {
                pd = (from a in this._LocalDS.Tables["tLicense"].AsEnumerable()
                      select new PlanData
                      {
                          subscriptionid = a.Field<string>("subscriptionid"),
                          projectname = a.Field<string>("projectname")
                      }).ToList();

                //foreach (DataRow item in this._LocalDS.Tables["tLicense"].Rows)
                //{
                //    selectListItems.Add(new SelectListItem
                //    {
                //        Value = item["subscriptionid"].ToString().Trim(),
                //        Text = item["projectname"].ToString().Trim()
                //    });
                //}
            }
            //if (this._LocalDS.Tables["tIssueService"] != null && this._LocalDS.Tables["tIssueService"].Rows.Count > 0)
            //{
            //    var service = (from a in this._LocalDS.Tables["tIssueService"].AsEnumerable()
            //                   select new PlanData
            //                   {
            //                       subscriptionid = a.Field<int>("id").ToString(),
            //                       projectname = a.Field<string>("servicename")
            //                   }).ToList();
            //    if (service != null && service.Count > 0)
            //    {
            //        service.ForEach(item => pd.Add(item));
            //    }
            //}
            if (pd != null && pd.Count > 0)
            {
                pd.ForEach(item => selectListItems.Add(new SelectListItem { Value = item.subscriptionid, Text = item.projectname }));
            }
            return selectListItems;
        }

        public List<SelectListItem> GetAMCClient()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem { Text = "[--Select--]", Value = "" });
            _oData.GetClientsForAmc("tClient");
            if (this._LocalDS.Tables["tClient"] != null && this._LocalDS.Tables["tClient"].Rows.Count > 0)
            {
                foreach (DataRow item in this._LocalDS.Tables["tClient"].Rows)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Value = item["id"].ToString().Trim(),
                        Text = item["companyname"].ToString().Trim()
                    });
                }
            }
            return selectListItems;
        }
        #endregion

        #region[whatsapp]
        public class parameters
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        internal void SendWhatsappMethod(string mobileNumber, string name, string ticketId, string subject, string templateName, string customerName = "")
        {
            var cusobj = new List<parameters>();
            cusobj.Add(new parameters { name = "name", value = name });
            cusobj.Add(new parameters { name = "ticketid", value = ticketId });
            cusobj.Add(new parameters { name = "subject", value = subject });
            if (customerName != "")
            {
                cusobj.Add(new parameters { name = "customer_name", value = customerName });
            }
            SendWhatsappMethod(mobileNumber, name, ticketId, templateName, cusobj);
        }
        internal void SendWhatsappMethod(string mobileNumber, string name, string ticketId, string nSupportManagerName)
        {
            var cusobj = new List<parameters>();
            cusobj.Add(new parameters { name = "name", value = name });
            cusobj.Add(new parameters { name = "ticketid", value = ticketId });
            cusobj.Add(new parameters { name = "supportmanager_name", value = nSupportManagerName });

            SendWhatsappMethod(mobileNumber, name, ticketId, "ticket_assigned", cusobj);
        }
        internal void SendWhatsappMethod(string mobileNumber, string name, string ticketId,string templateSubject, List<parameters> myCusobj = null)
        {
            var cusobj = new List<parameters>();
            if(myCusobj == null)
            {
                cusobj.Add(new parameters { name = "name", value = name });
                cusobj.Add(new parameters { name = "ticketid", value = ticketId });
            }
            else
                cusobj = myCusobj;

            var objdata = new
            {
                template_name = templateSubject,
                broadcast_name = templateSubject,
                parameters = cusobj
            };

            SendWhatsAppAsync(JsonConvert.SerializeObject(objdata), mobileNumber);
        }
        private async void SendWhatsAppAsync(string objdata, string mobileNumber)
        {
            string responseContent = "";
            try
            {
                var url = _baseConfig["Whatsapp:EndPoint"] != null ? _baseConfig["Whatsapp:EndPoint"].ToString() : "";
                var accessToken = _baseConfig["Whatsapp:token"] != null ? _baseConfig["Whatsapp:token"].ToString() : "";
                if (url != "" && accessToken != "")
                {
                    url += "api/v1/sendTemplateMessage?whatsappNumber=" + mobileNumber.Replace("+", "");

                    var content = new StringContent(objdata, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        using (HttpResponseMessage response = await client.PostAsync(url, content).ConfigureAwait(true))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                dynamic json = JObject.Parse(responseContent);
                                if (json.result == false)
                                {
                                    //add error to logg
                                    responseContent = json.info;
                                }
                            }
                            else
                            {
                                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                dynamic json = JObject.Parse(responseContent);
                                if (json.result == false)
                                {
                                    //add error to logg
                                    responseContent = json.info;
                                }
                            }
                        }
                    }
                }
                else
                {
                    responseContent = "Invalid API";
                }
            }
            catch (Exception ex)
            {
                responseContent = $"Error : {ex.Message}";
            }

            _log = new CreateLog(_baseEnv.WebRootPath);
            _log.WriteLog("WHATSAPP-ERROR : " + responseContent);
        }
        #endregion
    }
}

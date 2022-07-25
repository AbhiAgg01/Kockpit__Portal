using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using KockpitPortal.DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using KockpitPortal.Models.SuperAdmin;
using KockpitAuthenticator;
using KockpitUtility.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using KockpitPortal.Utility;
using System.IO;

namespace KockpitPortal.Controllers.Authentication
{
    public class LoginController : CookieController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        public IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CreateLog _log;
        IHostingEnvironment _env;

        public LoginController(IConfiguration config, IHttpContextAccessor httpContextAccessor, IHostingEnvironment env)
        {
            _config = config;
            oData = new DataPostgres(LocalDS, GetPostgreSQLConnectionString());
            this._httpContextAccessor = httpContextAccessor;
            _log = new CreateLog(env.WebRootPath);
            _env = env;
        }

        public IActionResult Index(string scontinue = "")
        {
            /////ViewBag.Logo = GetLogo();
            ViewBag.Scontinue = scontinue;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IFormCollection collection)
        {
            /////ViewBag.Logo = GetLogo();
            try
            {
                string strPassword = collection["password"].ToString().Trim();
                string strEmail = collection["email"].ToString().Trim();
                string strContinue = collection["continue"].ToString().Trim();
                oData.AuthenticateLogin(strEmail, Secure.Encryptdata(strPassword), "tAuthenticate");  //Remove this line 
               //oData.AuthenticateLogin(strEmail, strPassword, "tAuthenticate", GetSubDomain());
                if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                {
                    //Generate JWT Token
                    //var token = JwtManager.GenerateToken(this.LocalDS.Tables["tAuthenticate"].Rows[0]["Id"].ToString(),
                    //    this.LocalDS.Tables["tAuthenticate"].Rows[0]["EmailId"].ToString(),
                    //    this.LocalDS.Tables["tAuthenticate"].Rows[0]["Role"].ToString());

                    //set Cookie
                    //CookieOptions options = new CookieOptions();
                    //options.Expires = DateTime.Now.AddSeconds(10);
                    //options.Domain = "subdomain1.localhost";
                    //_httpContextAccessor.HttpContext.Response.Cookies.Append("jwt_token", token, options);

                    var user = this.LocalDS.Tables["tAuthenticate"].AsEnumerable().Select(row => new tblUsers
                    {
                        Id = row.Field<int>("Id"),
                        EmailId = row.Field<string>("EmailId"),
                        CompanyName = row.Field<string>("CompanyName"),
                        SchemaName = row.Field<string>("SchemaName"),
                        Logo = row.Field<string>("logo"),
                        SocketId = row.Field<string>("SocketId"),
                        ExpiredOn = row.Field<DateTime?>("ExpiredOn"),
                        Role = row.Field<string>("Role"),
                        ContactNo1 = row.Field<string>("contactno1"),
                        Password = row.Field<string>("Password")
                    }).FirstOrDefault();
                    
                    if (user.ExpiredOn != null && Convert.ToDateTime(user.ExpiredOn).Date < DateTime.Now.Date)
                    {
                        ViewData["error"] = "Account Expired !! Please contact to administrator";
                        return View("~/Views/Login/Index.cshtml");
                    }
                    else
                    {
                        //Generate JWT Token
                        var token = JwtHandler.GenerateToken(user.Id.ToString(), user.EmailId, user.Role, user.CompanyName, user.SchemaName);
                        Response.Cookies.Append("jwt_token", token, CookieOpt);

                        HttpContext.Session.SetInt32("SessionInfo_Id", user.Id);
                        HttpContext.Session.SetString("SessionInfo_EmailId", user.EmailId);
                        HttpContext.Session.SetString("SessionInfo_Password", user.Password);
                        HttpContext.Session.SetString("SessionInfo_CompanyName", user.CompanyName);
                        HttpContext.Session.SetString("SessionInfo_SchemaName", user.SchemaName);
                        HttpContext.Session.SetString("SessionInfo_logo", user.Logo == null ? string.Empty : user.Logo);
                        HttpContext.Session.SetString("SessionInfo_ExpiredOn", user.ExpiredOn == null ? "" : user.ExpiredOn.ToString());
                        HttpContext.Session.SetString("SessionInfo_SocketId", user.SocketId == null ? string.Empty : user.SocketId);
                        HttpContext.Session.SetString("SessionInfo_Role", user.Role);
                        HttpContext.Session.SetString("SessionInfo_ContactNumber", string.IsNullOrEmpty(user.ContactNo1) ? string.Empty : user.ContactNo1);

                        //maintain log
                        Log(user, "LOGIN");
                        if (!string.IsNullOrEmpty(strContinue))
                        {
                            //check for license
                            Uri myUri = new Uri(strContinue);
                            string host = myUri.Host + ":" + myUri.Port;
                            oData.PlanPurchaseGetAllActiveLicenseByUser(user.Id, "tPlanPurchaseGetAllActiveLicenseByUser");
                            if (this.LocalDS.Tables["tPlanPurchaseGetAllActiveLicenseByUser"] != null
                                && this.LocalDS.Tables["tPlanPurchaseGetAllActiveLicenseByUser"].Rows.Count > 0)
                            {
                                bool lfound = false;
                                string UserLicenseKey = "";
                                foreach (DataRow dr in this.LocalDS.Tables["tPlanPurchaseGetAllActiveLicenseByUser"].Rows)
                                {
                                    var offeringUrl = dr["ProjectStartUpLink"].ToString().Trim();
                                    Uri offUri = new Uri(offeringUrl);
                                    string offhost = offUri.Host + ":" + offUri.Port;
                                    if (offhost == host)
                                    {
                                        UserLicenseKey = dr["LicenseKey"].ToString().Trim();
                                        var jExtra = new JwtExtra
                                        {
                                            kid = UserLicenseKey,
                                            kpro = Convert.ToBoolean(dr["LicenseKey"].ToString().Trim()) ? "1" : "0"
                                        };
                                        Response.Cookies.Append("jwt_token_spec", JsonConvert.SerializeObject(jExtra), CookieOpt);
                                        lfound = true;
                                        break;
                                    }
                                }

                                if (lfound)
                                {
                                    oData.AuthenticateLicense(user.EmailId, UserLicenseKey, "tAuthenticate");
                                    if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                                    {
                                        var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
                                        if (expiryDate < DateTime.Now)
                                        {
                                            ViewData["error"] = "License Expired";
                                            return View("~/Views/Login/Index.cshtml");
                                        }
                                        else
                                            return Redirect(strContinue);
                                    }
                                }
                                else
                                {
                                    ViewData["error"] = "No License Found";
                                    return View("~/Views/Login/Index.cshtml");
                                }
                            }
                            else
                            {
                                ViewData["error"] = "No License Found";
                                return View("~/Views/Login/Index.cshtml");
                            }
                        }

             

                        if (user.Role == "SUPERADMIN")
                            return RedirectToAction("Index", "Home", new { });
                        else if (user.Role == "SUPPORTREPRESENTATIVE")
                            return RedirectToAction("ResolveIssue", "RaiseIssue", new { });
                        else if (user.Role == "SUPPORTMANAGER")
                            return RedirectToAction("ShowAllTickets", "RaiseIssue", new { });
                        else
                            return RedirectToAction("Dashboard", "Home", new { });
                    }
                }
                else
                {
                    ViewData["error"] = "Invalid Username and Password!";
                    return View("~/Views/Login/Index.cshtml");
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return View("~/Views/Login/Index.cshtml");
            }
        }

        public ActionResult Logout(string scontinue = "")
        {
            HttpContext.Session.Clear();            
            HttpContext.Session.Remove("SessionInfo_Id");
            HttpContext.Session.Remove("SessionInfo_EmailId");
            HttpContext.Session.Remove("SessionInfo_Password");
            HttpContext.Session.Remove("SessionInfo_CompanyName");
            HttpContext.Session.Remove("SessionInfo_SchemaName");
            HttpContext.Session.Remove("SessionInfo_SocketId");
            HttpContext.Session.Remove("SessionInfo_IsSuperAdmin");
            HttpContext.Session.Remove("SessionInfo_logo");
            HttpContext.Session.Remove("SessionInfo_ExpiredOn");
            HttpContext.Session.Remove("SessionInfo_ContactNumber");

            //Erase the data in the cookie
            //RemoveCookie("jwt_token");
            //RemoveCookie("kid");
            //Response.Cookies.Append("jwt_token", string.Empty, option);
            //Response.Cookies.Append("kid", string.Empty, option);
            ////Then delete the cookie
            //Response.Cookies.Delete("jwt_token");
            //Response.Cookies.Delete("kid");

            //RemoveCookie();

            //maintain log
            if (HttpContext.Session.GetString("SessionInfo_EmailId") != null)
            {
                Log(new tblUsers
                {
                    Id = HttpContext.Session.GetInt32("SessionInfo_Id").Value,
                    EmailId = HttpContext.Session.GetString("SessionInfo_EmailId"),
                    CompanyName = HttpContext.Session.GetString("SessionInfo_CompanyName")
                }, "LOGOUT");
            }

            return RedirectToAction("Index", "Login", new { scontinue = scontinue });
        }

        private void DeleteCookies()
        {
            foreach (var cookie in HttpContext.Request.Cookies)
            {
                Response.Cookies.Delete(cookie.Key);
            }
        }


        public void RemoveCookie()
        {
            CookieOptions myCookie = new CookieOptions();
            myCookie.Secure = true;
            myCookie.IsEssential = true;
            myCookie.Expires = DateTime.Now.AddDays(-1d);
            myCookie.Domain = ".kockpit.in";
            if (Request.Cookies["cookie"] != null)
            {
                Response.Cookies.Append("kid", "test", myCookie);
                Response.Cookies.Append("jwt_token", "test", myCookie);
            }
            //CookieOpt.Expires = DateTime.Now.AddDays(-1);
            //CookieOpt.Secure = true;
            //CookieOpt.IsEssential = true;
            //CookieOpt.Domain = ".kockpit.in";
            //Response.Cookies.Append(key, string.Empty, CookieOpt);
            //Response.Cookies.Delete(key);

            //Response.Cookies.Append("kid", UserLicenseKey, CookieOpt);
            //Response.Cookies.Append("jwt_token", token, CookieOpt);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string strEmail)
        {
            try
            {
                var registration = oData.UserSelectByEmail(strEmail.Trim());
                if (registration.Item1 == true)
                {
                    //code to send mail
                    string strMailOutPut = string.Empty;
                    string mailBody = System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "forgotpassword.html"))
                        ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "forgotpassword.html"))
                        : string.Empty;

                    var userPassword = GeneratePassword(8);
                    mailBody = string.Format(mailBody.ToString(), strEmail.Trim(), userPassword);
                    var res = SendMail(strEmail.Trim(), "New Credentials", mailBody, null, out strMailOutPut);
                    if (res)
                    {
                        //update Password
                        res = oData.ChangePassword(registration.Item2, Secure.Encryptdata(userPassword));
                        if (res)
                        {
                            ViewData["success"] = "Mail has been sent to your email address, Please check your email";
                        }
                        else
                        {
                            ViewData["error"] = "Error, Please try again.";
                        }
                    }
                    else
                    {
                        ViewData["error"] = "Error: " + strMailOutPut;
                    }
                }
                else
                {
                    ViewData["error"] = "Invalid emailid!";
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
            }
            return View("~/Views/Login/Index.cshtml");
        }

        public string GetPostgreSQLConnectionString(string strDatabase = "")
        {
            PostgreConnection postgreConnection = new PostgreConnection();
            postgreConnection.Host = _config["KockpitAuthenticatorPsql:Host"] != null ? _config["KockpitAuthenticatorPsql:Host"].ToString() : "";
            postgreConnection.Port = _config["KockpitAuthenticatorPsql:Port"] != null ? _config["KockpitAuthenticatorPsql:Port"].ToString() : "";
            postgreConnection.Username = _config["KockpitAuthenticatorPsql:Username"] != null ? _config["KockpitAuthenticatorPsql:Username"].ToString() : "";
            postgreConnection.Password = _config["KockpitAuthenticatorPsql:Password"] != null ? _config["KockpitAuthenticatorPsql:Password"].ToString() : "";
            postgreConnection.Database = string.IsNullOrEmpty(strDatabase)
                ? _config["KockpitAuthenticatorPsql:Database"] != null ? _config["KockpitAuthenticatorPsql:Database"].ToString() : ""
                : strDatabase;

            return string.IsNullOrEmpty(postgreConnection.Port)
                ? $"Host={postgreConnection.Host};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}"
                : $"Host={postgreConnection.Host};Port={postgreConnection.Port};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}";
        }

        private string GetSubDomain()
        {
            var curentDomain = Request.Host.Host;
            var subDomain = curentDomain.Split(".", StringSplitOptions.RemoveEmptyEntries).ToList().First();
            subDomain = (subDomain.ToLower() == "localhost") ? "" : (subDomain.ToLower() == "configurator") ? "public" : subDomain;
            return subDomain;
        }

        private string GetLogo()
        {
            string strLogo = "~/img/logofull.svg";
            oData.GetLogo(GetSubDomain(), "tLogo");
            if (this.LocalDS.Tables["tLogo"] != null && this.LocalDS.Tables["tLogo"].Rows.Count > 0)
            {
                strLogo = this.LocalDS.Tables["tLogo"].Rows[0][0].ToString().Trim();
            }
            return strLogo;
        }

        private void Log(tblUsers user, string action)
        {
            var logging = new Logging
            {
                Action = action,
                UserId = user.Id,
                EmailId = user.EmailId,
                Username = user.CompanyName
            };
            _log.WriteLog(JsonConvert.SerializeObject(logging).ToString().Trim());
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
        public bool SendMail(string strReceiver, string strSubject, string strBody, List<string> ccEmails, out string sentMsg)
        {
            try
            {
                string strSenderEmail = _config["MailCredential:Email"] != null ? _config["MailCredential:Email"].ToString() : "";
                string strSenderPassword = _config["MailCredential:Password"] != null ? _config["MailCredential:Password"].ToString() : "";
                int strSenderPort = _config["MailCredential:Port"] != null ? Convert.ToInt32(_config["MailCredential:Port"].ToString()) : 587;
                string strSenderHost = _config["MailCredential:Host"] != null ? _config["MailCredential:Host"].ToString() : "";
                bool lEnableSsl = _config["MailCredential:EnableSsl"] != null ? Convert.ToBoolean(_config["MailCredential:EnableSsl"].ToString()) : true;

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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class UserManagementController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataPostgres oDataConfigurator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;

        public UserManagementController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            oDataConfigurator = new DataPostgres(LocalDS, ConfiguratorConnection());
            _env = env;
        }

        public IActionResult Index()
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");


            List<tblUsers> users = new List<tblUsers>();
            try
            {
                oDataAuthenticator.ClientUserGetAll(USERID, "tClientUsers");
                if (this.LocalDS.Tables["tClientUsers"] != null
                    && this.LocalDS.Tables["tClientUsers"].Rows.Count > 0)
                {
                    users = this.LocalDS.Tables["tClientUsers"].AsEnumerable().Select(row =>
                    new tblUsers
                    {
                        Id = row.Field<int>("Id"),
                        EmailId = row.Field<string>("EmailId"),
                        Password = row.Field<string>("Password"),
                        CompanyName = row.Field<string>("CompanyName"),
                        SchemaName = row.Field<string>("SchemaName"),
                        ExpiredOn = row.Field<DateTime>("ExpiredOn"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        ActiveStatus = row.Field<bool>("ActiveStatus"),
                        Logo = string.IsNullOrEmpty(row.Field<string>("Logo")) ? "~/assets/download.png" : row.Field<string>("Logo"),
                        EmpCode = row.Field<string>("empcode"),
                        ContactNo1 = row.Field<string>("contactno1"),
                        Contact2 = row.Field<string>("contactno2"),
                        Address = row.Field<string>("address"),
                        ClientConnected = //Sockets.Where(c => c.Session == row.Field<string>("SocketId")).Any() ? true : 
                        false,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            tblUsersViewModel usersViewModel = new tblUsersViewModel();
            usersViewModel.users = users;
            ViewData["PageTitle"] = "User Management";
            ViewData["PageId"] = "SA007";
            return View(usersViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblUsersViewModel viewModel)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            string strFilePath = string.Empty;
            Response response = new Response();
            response.success = true;

            tblUsers oModel = viewModel.user;
            oModel.Logo = string.Empty;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    //check for subdomain exists
                    //////if (!CallHttpWebRequest($"http://{oModel.SchemaName}.configurator.kockpit.in/"))
                    //////{
                    //////    TempData["error"] = $"Error : Sub domain not configured! Please create new sub domain for this client";
                    //////    return RedirectToAction("Index", "UserManagement");
                    //////}
                    //////else if(!CallHttpsWebRequest($"https://{oModel.SchemaName}.configurator.kockpit.in/"))
                    //////{
                    //////    TempData["error"] = $"Error : Sub domain not configured! Please create new sub domain for this client";
                    //////    return RedirectToAction("Index", "UserManagement");
                    //////}


                    //check for valid email address
                    if (!IsValidEmail(oModel.EmailId) && oModel.Id == 0)
                    {
                        TempData["error"] = $"Error : Invalid email address";
                        return RedirectToAction("Index", "UserManagement");
                    }

                    //validate for duplicate username and schema
                    var res = oDataAuthenticator.ClientUserCheckDuplicacy(oModel.Id, oModel.EmailId, oModel.SchemaName, oModel.CompanyName,oModel.EmpCode,oModel.ContactNo1,oModel.Contact2);
                    if (res.Item1 == false)
                    {
                        TempData["error"] = $"Error : {res.Item2}";
                        return RedirectToAction("Index", "UserManagement");
                    }

                    if (oModel.Id != 0)
                    {
                        oDataAuthenticator.ClientUserGetById(oModel.Id, "tClientById");
                        if (this.LocalDS.Tables["tClientById"] != null && this.LocalDS.Tables["tClientById"].Rows.Count > 0)
                        {
                            oModel.Logo = this.LocalDS.Tables["tClientById"].Rows[0]["Logo"].ToString();
                            oModel.Password = this.LocalDS.Tables["tClientById"].Rows[0]["Password"].ToString();
                        }
                    }

                    if (oModel.file != null && oModel.file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(oModel.file.FileName);
                        strFilePath = Path.Combine(_env.WebRootPath, "Resource/Logo", fileName);
                        if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                        }
                        using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                        {
                            oModel.file.CopyTo(fileStream);
                        }
                        oModel.Logo = Path.Combine("~/Resource/Logo/", fileName);
                    }

                    if (oModel.Id != 0)
                    {
                        //alter username
                        //update entry
                        if (response.success)
                            response.success = oDataAuthenticator.ClientUserUpdate(oModel.Id, Convert.ToDateTime(oModel.ExpiredOn), oModel.ActiveStatus, oModel.Logo, "ADMIN", oModel.EmpCode, oModel.ContactNo1, oModel.Contact2,oModel.Address);

                        if (response.success)
                        {
                            TempData["success"] = "Client Updated successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                            TempData["error"] = "Something wrong while updating client";
                    }
                    else
                    {
                        oModel.SocketId = Guid.NewGuid().ToString().Trim();
                        //generate password
                        oModel.Password = Secure.Encryptdata(GeneratePassword(8));
                        oModel.CreatedOn = DateTime.Now;
                        //oModel.ActiveStatus = true;
                        ////////////response.success = oDataConfigurator.ClientUserCreate(oModel.EmailId, oModel.Password);
                        ////////////if (response.success)
                        ////////////    response.success = oDataConfigurator.ClientUserSchemaCreate(oModel.SchemaName, oModel.EmailId);

                        ////////////if (response.success)
                        ////////////{
                        ////////////    var schemaScript = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "configurator_schema.sql")))
                        ////////////   ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "configurator_schema.sql"))
                        ////////////   : string.Empty;
                        ////////////    schemaScript = string.Format(schemaScript, oModel.EmailId, oModel.SchemaName);
                        ////////////    response.success = oDataConfigurator.ClientUserSchemaStructure(schemaScript);
                        ////////////}

                        if (response.success)
                        {
                            //var result = Task.Run(async () => await CreateContact(oModel.ContactNo1.Replace("+", ""), oModel.CompanyName)).Result;
                            response.success = oDataAuthenticator.ClientUserInsert(oModel.EmailId, oModel.Password, oModel.CompanyName, oModel.SchemaName,
                                    Convert.ToDateTime(oModel.ExpiredOn), oModel.CreatedOn, oModel.ActiveStatus, oModel.Logo, oModel.SocketId, "ADMIN", USERID, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2,oModel.Address);
                        }

                        if (response.success)
                        {
                            //Send Mail to USER
                            string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html")))
                           ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html"))
                           : string.Empty;

                            //var clientSocketLink = $"wss://localhost:44393/ws?session={oModel.SocketId}";

                            string sMailOutput = "";
                            string sMailSubject = "Kockpit Portal Credentials";
                            string sMailBody = string.Format(mailBody.ToString(), oModel.EmailId, Secure.Decryptdata(oModel.Password), oModel.SocketId);

                            List<string> ccemails = null;
                            if (!string.IsNullOrEmpty(oModel.CCMailIds))
                            {
                                ccemails = oModel.CCMailIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                            }

                            if (!SendMail(oModel.EmailId, sMailSubject, sMailBody, ccemails, out sMailOutput))
                            {
                                //TempData["error"] = "Error on mail Sent: " + sMailOutput;
                            }

                            TempData["success"] = "Client created successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                            TempData["error"] = "Something wrong while creating client";
                    }
                }
                catch (TransactionException ex)
                {
                    if (System.IO.File.Exists(strFilePath))
                        System.IO.File.Delete(strFilePath);
                    TempData["error"] = $"Error : {ex.Message}";
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(strFilePath))
                        System.IO.File.Delete(strFilePath);
                    TempData["error"] = $"Error : {ex.Message}";
                    transaction.Dispose();
                }
            }

            return RedirectToAction("Index", "UserManagement");
        }



        public IActionResult Remove(int nId, string strSchemaName, string strEmailId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    ///1. delete Postgres User
                    ///2. delete postgres schema
                    ///4. insert into tblUsers
                    ///5. send mail to user for userid and password
                    ///

                    //check for data is exists
                    ////if (oDataConfigurator.ClientUserCanRemove(strSchemaName))
                    ////{
                        ////var res = oDataConfigurator.ClientUserSchemaRemove(strSchemaName, strEmailId);
                        var res = oDataAuthenticator.ClientUserRemove(nId);
                        if (res)
                        {
                            TempData["success"] = "Client deleted successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                        {
                            TempData["error"] = $"Error while removing the client";
                        }
                    ////}
                    ////else
                    ////{
                    ////    TempData["error"] = $"Client have data in there schema So, you cannot delete this client.";
                    ////}
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
            return RedirectToAction("Index", "UserManagement");
        }

        private bool CallHttpsWebRequest(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
        private bool CallHttpWebRequest(string URL)
        {
            try
            {
                string sAddress = URL;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sAddress);
                req.Accept = "text/xml,text/plain,text/html";
                req.Method = "GET";
                HttpWebResponse result = (HttpWebResponse)req.GetResponse();
                Stream ReceiveStream = result.GetResponseStream();
                StreamReader reader = new StreamReader(ReceiveStream, System.Text.Encoding.ASCII);
                string respHTML = reader.ReadToEnd();
                reader.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public JsonResult CheckURL(string strSchemaName)
        {
            Response response = new Response();
            try
            {
                string Url = "http://" + strSchemaName + ".configurator.kockpit.in/";
                if (CallHttpWebRequest(Url))
                {
                    response.success = true;
                    response.msg = "Sub domain exists.";
                }
                else
                {
                    Url = "https://" + strSchemaName + ".configurator.kockpit.in/";
                    if (CallHttpsWebRequest(Url))
                    {
                        response.success = true;
                        response.msg = "Sub domain exists.";
                    }
                    else
                    {
                        response.success = false;
                        response.msg = "Sub domain not configured! Please create new sub domain for this client";
                    }
                }
            }
            catch (Exception ex)
            {
                response.msg = "Error: " + ex.Message;
            }

            return Json(response);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class EmployeeController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        public EmployeeController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;

        }
        public IActionResult Index()
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            List<tblUsers> users = new List<tblUsers>();
            try
            {
                oDataAuthenticator.EmployeeUserGetAll(USERID, "tClientUsers");
                if (this.LocalDS.Tables["tClientUsers"] != null
                    && this.LocalDS.Tables["tClientUsers"].Rows.Count > 0)
                {
                    users = this.LocalDS.Tables["tClientUsers"].AsEnumerable().Select(row =>
                    new tblUsers
                    {
                        Id = row.Field<int>("Id"),
                        EmailId = row.Field<string>("EmailId"),
                        Role = row.Field<string>("role"),
                        EmpCode = row.Field<string>("empcode"),
                        ContactNo1 = row.Field<string>("contactno1"),
                        Contact2 = row.Field<string>("contactno2"),
                        Password = row.Field<string>("Password"),
                        CompanyName = row.Field<string>("CompanyName"),
                        SchemaName = row.Field<string>("SchemaName"),
                        ExpiredOn = row.Field<DateTime>("ExpiredOn"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        ActiveStatus = row.Field<bool>("ActiveStatus"),
                        Logo = string.IsNullOrEmpty(row.Field<string>("Logo")) ? "~/assets/download.png" : row.Field<string>("Logo")
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
            ViewData["PageId"] = "SA006";
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
            oModel.SchemaName = string.Empty;
            oModel.SocketId = string.Empty;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    DateTime nextYear = DateTime.Now.AddYears(1);

                    //check for valid email address
                    if (!IsValidEmail(oModel.EmailId) && oModel.Id == 0)
                    {
                        TempData["error"] = $"Error : Invalid email address";
                        return RedirectToAction("Index", "Employee");
                    }

                    //validate for duplicate username and schema
                    var res = oDataAuthenticator.ClientUserCheckDuplicacy(oModel.Id, oModel.EmailId, oModel.SchemaName, oModel.CompanyName, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2);
                    if (res.Item1 == false)
                    {
                        TempData["error"] = $"Error : {res.Item2}";
                        return RedirectToAction("Index", "Employee");
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
                        strFilePath = Path.Combine(_env.WebRootPath, "Resource/Employee", fileName);
                        if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                        }
                        using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                        {
                            oModel.file.CopyTo(fileStream);
                        }
                        oModel.Logo = Path.Combine("~/Resource/Employee/", fileName);
                    }

                    if (oModel.Id != 0)
                    {
                        if (response.success)
                        {
                            if (oModel.IsSupportManager)
                                oModel.Role = "SUPPORTMANAGER";
                            else
                                oModel.Role = "SUPPORTREPRESENTATIVE";
                            response.success = oDataAuthenticator.ClientUserUpdate(oModel.Id, nextYear, oModel.ActiveStatus, oModel.Logo, oModel.Role, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2, null);
                        }

                        if (response.success)
                        {
                            TempData["success"] = "Employee Updated successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                            TempData["error"] = "Something wrong while updating client";
                    }
                    else
                    {
                        oModel.SchemaName = HttpContext.Session.GetString("SessionInfo_SchemaName").ToString().Trim();
                        var test = HttpContext.Session.GetString("SessionInfo_ExpiredOn").ToString().Trim();
                        //oModel.ExpiredOn = Convert.ToDateTime(HttpContext.Session.GetString("SessionInfo_ExpiredOn").ToString().Trim());
                        if (string.IsNullOrEmpty(oModel.Logo))
                            oModel.Logo = HttpContext.Session.GetString("SessionInfo_logo").ToString().Trim();
                        oModel.SocketId = HttpContext.Session.GetString("SessionInfo_SocketId").ToString().Trim();

                        oModel.Password = Secure.Encryptdata(GeneratePassword(8));
                        oModel.CreatedOn = DateTime.Now;
                       // oModel.ActiveStatus = true;

                        if (response.success)
                        {
                            if (oModel.IsSupportManager)
                                oModel.Role = "SUPPORTMANAGER";
                            else
                                oModel.Role = "SUPPORTREPRESENTATIVE";

                            //var result = Task.Run(async () => await CreateContact(oModel.ContactNo1.Replace("+", ""), oModel.CompanyName)).Result;
                            response.success = oDataAuthenticator.ClientUserInsert(oModel.EmailId, oModel.Password, oModel.CompanyName, oModel.SchemaName,
                                nextYear, oModel.CreatedOn, oModel.ActiveStatus, oModel.Logo, oModel.SocketId, oModel.Role, USERID, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2, null);
                        }

                        if (response.success)
                        {
                            string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html")))
                           ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html"))
                           : string.Empty;

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

                            TempData["success"] = "Employee created successfully";
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

            return RedirectToAction("Index", "Employee");
        }



        public IActionResult Remove(int nId, string strSchemaName, string strEmailId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
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
            return RedirectToAction("Index", "Employee");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.Cart;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KockpitPortal.Controllers.Admin
{
    public class SubUserManagementController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        public SubUserManagementController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;

        }
        public IActionResult Index()
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            List<tblUsers> users = new List<tblUsers>();
            try
            {
                oDataAuthenticator.GetSubUsers(USERID, "tClientUsers");
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
                        ContactNo1 = row.Field<string>("contactno1"),
                        Contact2 = row.Field<string>("contactno2"),
                        ActiveStatus = row.Field<bool>("ActiveStatus"),
                        EmpCode=row.Field<string>("empcode"),
                        Logo = string.IsNullOrEmpty(row.Field<string>("Logo")) ? "~/assets/download.png" : row.Field<string>("Logo")
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
            }

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

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            tblUsersViewModel usersViewModel = new tblUsersViewModel();
            usersViewModel.users = users;
            ViewData["PageTitle"] = "User Management";
            ViewData["PageId"] = "OA002";
            return View(usersViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblUsersViewModel viewModel)
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

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
                    //check for valid email address
                    if (!IsValidEmail(oModel.EmailId) && oModel.Id == 0)
                    {
                        TempData["error"] = $"Error : Invalid email address";
                        return RedirectToAction("Index", "SubUserManagement");
                    }

                    //validate for duplicate username and schema
                    var res = oDataAuthenticator.ClientUserCheckDuplicacy(oModel.Id, oModel.EmailId, oModel.SchemaName, oModel.CompanyName,oModel.EmpCode, oModel.ContactNo1, oModel.Contact2);
                    if (res.Item1 == false)
                    {
                        TempData["error"] = $"Error : {res.Item2}";
                        return RedirectToAction("Index", "SubUserManagement");
                    }

                    if (oModel.Id != 0)
                    {
                        oDataAuthenticator.ClientUserGetById(oModel.Id, "tClientById");
                        if (this.LocalDS.Tables["tClientById"] != null && this.LocalDS.Tables["tClientById"].Rows.Count > 0)
                        {
                            oModel.Logo = this.LocalDS.Tables["tClientById"].Rows[0]["Logo"].ToString();
                            oModel.Password = this.LocalDS.Tables["tClientById"].Rows[0]["Password"].ToString();
                            oModel.Role = this.LocalDS.Tables["tClientById"].Rows[0]["role"].ToString();
                            oModel.ExpiredOn = Convert.ToDateTime(this.LocalDS.Tables["tClientById"].Rows[0]["expiredon"].ToString());
                        }
                    }

                    if (oModel.file != null && oModel.file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(oModel.file.FileName);
                        strFilePath = Path.Combine(_env.WebRootPath, "Resource/SubAdmin", fileName);
                        if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                        }
                        using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                        {
                            oModel.file.CopyTo(fileStream);
                        }
                        oModel.Logo = Path.Combine("~/Resource/SubAdmin/", fileName);
                    }

                    if (oModel.Id != 0)
                    {
                        if (response.success)
                            response.success = oDataAuthenticator.ClientUserUpdate(oModel.Id, Convert.ToDateTime(oModel.ExpiredOn), oModel.ActiveStatus, oModel.Logo, oModel.Role, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2, null);
                        if (response.success)
                        {
                            TempData["success"] = "User Updated successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                            TempData["error"] = "Something wrong while updating client";
                    }
                    else
                    {
                        oModel.SchemaName = HttpContext.Session.GetString("SessionInfo_SchemaName").ToString().Trim();
                        oModel.ExpiredOn = Convert.ToDateTime(HttpContext.Session.GetString("SessionInfo_ExpiredOn").ToString().Trim());
                        //oModel.Logo = HttpContext.Session.GetString("SessionInfo_logo").ToString().Trim();
                        oModel.SocketId = HttpContext.Session.GetString("SessionInfo_SocketId").ToString().Trim();

                        oModel.Password = Secure.Encryptdata(GeneratePassword(8));
                        oModel.CreatedOn = DateTime.Now;
                        oModel.ActiveStatus = true;
                        if (response.success)
                            response.success = oDataAuthenticator.ClientUserInsert(oModel.EmailId, oModel.Password, oModel.CompanyName, oModel.SchemaName,
                                Convert.ToDateTime(oModel.ExpiredOn), oModel.CreatedOn, oModel.ActiveStatus, oModel.Logo, oModel.SocketId, "SUBADMIN", USERID, oModel.EmpCode, oModel.ContactNo1, oModel.Contact2, null);
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

                            TempData["success"] = "User created successfully";
                            transaction.Complete();
                            transaction.Dispose();
                        }
                        else
                            TempData["error"] = "Something wrong while creating user";
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

            return RedirectToAction("Index", "SubUserManagement");
        }


        public IActionResult Remove(int nId)
        {
            if (!ISADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res = oDataAuthenticator.ClientUserRemove(nId);
                    if (res)
                    {
                        TempData["success"] = "User deleted successfully";
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
            return RedirectToAction("Index", "SubUserManagement");
        }



        public IActionResult SendBulkMail(int userid = 0)
        {
            try
            {
                //code to generate new password and send the bulk mail 
                List<tblUsers> users = new List<tblUsers>();
                oDataAuthenticator.SubUserGetAll(USERID, "tClientUsers");
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
                        Logo = row.Field<string>("Logo")
                    }).ToList();
                }


                if(users != null && users.Count > 0)
                {
                    if(userid == 0)
                    {
                        foreach (var item in users)
                        {
                            //code to generate new password
                            var EmailId = item.EmailId;
                            var Password = Secure.Encryptdata(GeneratePassword(8));
                            var SocketId = "";

                            var res = oDataAuthenticator.UpdatePassword(item.Id, Password);
                            if (res)
                            {
                                //code to send email to users
                                string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html")))
                                   ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html"))
                                   : string.Empty;

                                string sMailOutput = "";
                                string sMailSubject = "Kockpit Portal Credentials";
                                string sMailBody = string.Format(mailBody.ToString(), EmailId, Secure.Decryptdata(Password), SocketId);

                                List<string> ccemails = null;
                                if (!SendMail(EmailId, sMailSubject, sMailBody, ccemails, out sMailOutput))
                                {
                                }
                            }
                        }
                        TempData["success"] = "Mail has been sent to all users.";
                    }
                    else
                    {
                        var currentUser = users.Where(c => c.Id == userid).FirstOrDefault();
                        if(currentUser != null)
                        {
                            //code to generate new password
                            var EmailId = currentUser.EmailId;
                            var Password = Secure.Encryptdata(GeneratePassword(8));
                            var SocketId = "";
                            var res = oDataAuthenticator.UpdatePassword(currentUser.Id, Password);

                            if (res)
                            {
                                //code to send email to users
                                string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html")))
                                   ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html"))
                                   : string.Empty;

                                string sMailOutput = "";
                                string sMailSubject = "Kockpit Portal Credentials";
                                string sMailBody = string.Format(mailBody.ToString(), EmailId, Secure.Decryptdata(Password), SocketId);

                                List<string> ccemails = null;
                                if (!SendMail(EmailId, sMailSubject, sMailBody, ccemails, out sMailOutput))
                                {
                                }

                                TempData["success"] = "Mail has been sent to all users.";
                            }
                            else
                            {
                                TempData["error"] = "Error while sending mail to user";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error : { ex.Message }";
            }

            return RedirectToAction("Index", "SubUserManagement");
        }

    }
}

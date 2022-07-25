using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.SuperAdmin;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Controllers.Authentication
{
    public class ChangePasswordController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public ChangePasswordController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            _config = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
            _oData = oDataAuthenticator;
            _LocalDS = LocalDS;

        }

        public IActionResult Index()
        {
            tblUsers users = new tblUsers();
            oDataAuthenticator.GetUsersinfo(USERID, "tUserInfo");
            if (this.LocalDS.Tables["tUserInfo"] != null && this.LocalDS.Tables["tUserInfo"].Rows.Count > 0)
            {
                users = this.LocalDS.Tables["tUserInfo"].AsEnumerable().Select(row =>
                    new tblUsers
                    {
                        Id = row.Field<int>("id"),
                        EmailId = row.Field<string>("emailid"),
                        CompanyName = row.Field<string>("companyname"),
                        Role = row.Field<string>("role")
                    }).FirstOrDefault();
            }

            //ViewBag.Users = users;
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            return View(users);
        }

        [HttpPost]
        public IActionResult Index(tblUsers users)
        {
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res =  oDataAuthenticator.UpdatePassword(users.Id, Secure.Encryptdata(users.Password));
                    if (res)
                    {
                        TempData["success"] = "Succesfully updated the password";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        TempData["error"] = "Error while changing password, Please try again.";
                        transaction.Dispose();
                    }

                }catch(TransactionException e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
                catch(Exception e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
            }
            return RedirectToAction("Index");
        }

        public JsonResult CheckOldPassword(string strOldPassword)
        {
            bool isValid = false;
            oDataAuthenticator.VerifyPassword(USERID, Secure.Encryptdata(strOldPassword), "tValidatePassword");
            if (this.LocalDS.Tables["tValidatePassword"] != null && this.LocalDS.Tables["tValidatePassword"].Rows.Count > 0)
            {
                isValid = true;
            }
            return Json(isValid);
        }
    }
}

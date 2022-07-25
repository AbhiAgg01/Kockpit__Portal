using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Controllers.Profile
{
    public class ProfileController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public ProfileController(IConfiguration config, IHostingEnvironment env)
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
            var user = new tblUsers();
            oDataAuthenticator.GetUsersinfo(USERID,"tUserInfo");
            if (this.LocalDS.Tables["tUserInfo"] != null 
                && this.LocalDS.Tables["tUserInfo"].Rows.Count > 0)
            {
                user = this.LocalDS.Tables["tUserInfo"].AsEnumerable().Select(row => new tblUsers
                {
                    Id = row.Field<int>("Id"),
                    EmailId = row.Field<string>("EmailId"),
                    CompanyName = row.Field<string>("CompanyName"),
                    SchemaName = row.Field<string>("SchemaName"),
                    Logo = row.Field<string>("logo"),
                    SocketId = row.Field<string>("SocketId"),
                    Role = row.Field<string>("Role"),
                    EmpCode = !string.IsNullOrEmpty(row.Field<string>("empcode")) ? row.Field<string>("empcode") : "",
                    ContactNo1 = !string.IsNullOrEmpty(row.Field<string>("contactno1")) ? row.Field<string>("contactno1"):"",
                    Contact2 = !string.IsNullOrEmpty(row.Field<string>("contactno2")) ? row.Field<string>("contactno2") : "",
                    Address = !string.IsNullOrEmpty(row.Field<string>("address")) ? row.Field<string>("address") : ""
                }).FirstOrDefault();
            }
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(tblUsers users)
        {
            var strFilePath = "";
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    if(users.file != null && users.file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(users.file.FileName);
                        if(users.Role.Trim() == "SUPPORTMANAGER" || users.Role.Trim() == "SUPPORTREPRESENTATIVE")
                        {
                            strFilePath = Path.Combine(_env.WebRootPath, "Resource/Employee", fileName);
                            users.Logo = Path.Combine( "~/Resource/Employee/", fileName);
                        }
                        else if(users.Role == "ADMIN")
                        {
                            strFilePath = Path.Combine(_env.WebRootPath, "Resource/Logo", fileName);
                            users.Logo = Path.Combine( "~/Resource/Logo/", fileName);
                        }
                        else if(users.Role == "SUBADMIN")
                        {
                            strFilePath = Path.Combine(_env.WebRootPath, "Resource/SubAdmin", fileName);
                            users.Logo = Path.Combine("~/Resource/SubAdmin/", fileName);
                        }
                        else
                        {
                            strFilePath = Path.Combine(_env.WebRootPath, "img", fileName);
                            users.Logo = Path.Combine("~/img/", fileName);
                        }
                        if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                        }
                        using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                        {
                            users.file.CopyTo(fileStream);
                        }
                    }

                    var res = oDataAuthenticator.UpdateUserProfile(users.Id, users.ContactNo1, users.Contact2, users.Address, users.Logo);
                    if (res)
                    {
                        transaction.Complete();
                        transaction.Dispose();
                        TempData["success"] = "profile update sucessfully";
                    }
                    else
                    {
                        transaction.Dispose();
                        TempData["error"] = "can't update profile";
                    }

                }
                catch(TransactionException e)
                {
                    transaction.Dispose();
                    TempData["error"] = e.Message;
                }
                catch(Exception e)
                {
                    transaction.Dispose();
                    TempData["error"] = e.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}

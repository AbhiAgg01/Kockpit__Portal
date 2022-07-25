using KockpitPortal.DataAccessLayer;
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
    public class ServiceController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public ServiceController(IConfiguration config, IHostingEnvironment env)
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
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            var services = new List<tblService>();
            tblServiceViewModel tblServiceViewModel = new tblServiceViewModel();
            oDataAuthenticator.GetServices("tServices");
            if (this.LocalDS.Tables["tServices"] != null
                && this.LocalDS.Tables["tServices"].Rows.Count > 0)
            {
                services = this.LocalDS.Tables["tServices"].AsEnumerable().Select(rows =>
                new tblService
                {
                    Id = rows.Field<int>("id"),
                    Description = rows.Field<string>("description"),
                    ServiceName = rows.Field<string>("servicename"),
                    ActiveStatus = rows.Field<bool>("activestatus"),
                    CreatedOn = rows.Field<DateTime>("createdon")
                }).ToList();
            }
            tblServiceViewModel.services = services;
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageTitle"] = "Services";
            ViewData["PageId"] = "SA013";
            return View(tblServiceViewModel);
        }

        [HttpPost]
        public IActionResult Upsert(tblService service)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var success = oDataAuthenticator.ServiceUpsert(service);
                    if (success)
                    {
                        if (service.Id != 0)
                            TempData["success"] = "Updated successfully";
                        else
                            TempData["success"] = "Created successfully";
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
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oDataAuthenticator.DeleteService(nId);
                    TempData["success"] = "Deleted successfully";
                    transaction.Complete();
                    transaction.Dispose();
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
            return RedirectToAction("Index");
        }
    }
}

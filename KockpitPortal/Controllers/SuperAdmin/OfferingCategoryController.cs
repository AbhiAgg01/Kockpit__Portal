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

namespace KockpitPortal.Controllers
{
    public class OfferingCategoryController : BaseController
    {

        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public OfferingCategoryController(IConfiguration config, IHostingEnvironment env)
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
            var offeringcategories = new List<tblOfferingCategory>();
            tblOfferingCategoryViewModel offeringCategoryViewModel = new tblOfferingCategoryViewModel();
            oDataAuthenticator.GetOfferingCategory("tOfferingCategory");
            if (this.LocalDS.Tables["tOfferingCategory"] != null
                && this.LocalDS.Tables["tOfferingCategory"].Rows.Count > 0)
            {
                offeringcategories = this.LocalDS.Tables["tOfferingCategory"].AsEnumerable().Select(rows =>
                new tblOfferingCategory
                {
                    Id = rows.Field<int>("id"),
                    Description = rows.Field<string>("description"),
                    OfferingCategory = rows.Field<string>("Offeringcategory"),
                    ActiveStatus = rows.Field<bool>("activestatus"),
                    IsPro = rows.Field<bool>("ispro"),
                    CreatedOn = rows.Field<DateTime>("createdon"),
                }).ToList();
            }
            offeringCategoryViewModel.offeringCategories = offeringcategories;
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageTitle"] = "Offering Category";
            ViewData["PageId"] = "SA012";
            return View(offeringCategoryViewModel);
        }

        [HttpPost]
        public IActionResult Upsert(tblOfferingCategory offeringCategory)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var success = oDataAuthenticator.OfferingCategoryUpsert(offeringCategory);
                    if (success)
                    {
                        if (offeringCategory.Id != 0)
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
                    oDataAuthenticator.DeleteOfferingCategory(nId);
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

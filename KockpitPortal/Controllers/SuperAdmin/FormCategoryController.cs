using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class FormCategoryController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public FormCategoryController(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;

            _oData = oData;
            _LocalDS = LocalDS;
        }

        public IActionResult Index()
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            List<tblFormCategory> tblFormCategorys = new List<tblFormCategory>();
            oData.FormCategoryGetAll("tFormCategoryGetAll");
            if (this.LocalDS.Tables["tFormCategoryGetAll"] != null
                && this.LocalDS.Tables["tFormCategoryGetAll"].Rows.Count > 0)
            {
                tblFormCategorys = this.LocalDS.Tables["tFormCategoryGetAll"].AsEnumerable().Select(row =>
                new tblFormCategory
                {
                    Id = Convert.ToInt32(row["Id"].ToString().Trim()),
                    ProjectId = Convert.ToInt32(row["ProjectId"].ToString().Trim()),
                    ProjectName = row.Field<string>("ProjectName"),
                    CategoryName = row.Field<string>("CategoryName"),
                }).ToList();
            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            ViewBag.ProjectList = ProjectList();
            tblFormCategoryViewModel viewModel = new tblFormCategoryViewModel();
            viewModel.formCategories = tblFormCategorys;

            ViewData["PageId"] = "SA004";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(tblFormCategoryViewModel viewModel)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            tblFormCategory oModel = viewModel.formCategory;
            oModel.CreatedOn = DateTime.Now;
            oModel.ActiveStatus = true;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.FormCategoryCheckDuplicacy(new tblFormCategory { Id = oModel.Id, CategoryName = oModel.CategoryName, ProjectId = oModel.ProjectId }, "tDuplicate");
                    if (this.LocalDS.Tables["tDuplicate"] != null
                        && this.LocalDS.Tables["tDuplicate"].Rows.Count > 0)
                    {
                        TempData["error"] = $"Already exists";
                    }
                    else
                    {
                        oData.FormCategoryUpsert(oModel);
                        if (oModel.Id != 0)
                            TempData["success"] = "Updated successfully";
                        else
                            TempData["success"] = "Saved successfully";
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

            return RedirectToAction("Index", "FormCategory");
        }

        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.FormCategoryGetById(nId, "tFormCategoryGetById");
                    var oModel = this.LocalDS.Tables["tFormCategoryGetById"].AsEnumerable()
                        .Select(row => new tblFormCategory
                        {
                            Id = row.Field<int>("Id"),
                            ProjectId = row.Field<int>("ProjectId"),
                            CategoryName = row.Field<string>("CategoryName"),
                        }).FirstOrDefault();

                    oModel.ActiveStatus = false;
                    oData.FormCategoryUpsert(oModel);
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
            return RedirectToAction("Index", "Module");
        }
    }
}

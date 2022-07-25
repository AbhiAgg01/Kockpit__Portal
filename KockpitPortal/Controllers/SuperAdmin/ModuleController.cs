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
    public class ModuleController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public ModuleController(IConfiguration config, IHostingEnvironment env)
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

            List<tblModule> tblModules = new List<tblModule>();
            oData.ModuleGetAll("tModuleGetAll");
            if (this.LocalDS.Tables["tModuleGetAll"] != null
                && this.LocalDS.Tables["tModuleGetAll"].Rows.Count > 0)
            {
                tblModules = this.LocalDS.Tables["tModuleGetAll"].AsEnumerable().Select(row =>
                new tblModule
                {
                    Id = Convert.ToInt32(row["Id"].ToString().Trim()),
                    ProjectId = Convert.ToInt32(row["ProjectId"].ToString().Trim()),
                    ProjectName = row.Field<string>("ProjectName"),
                    ModuleName = row.Field<string>("ModuleName"),
                    Description = row.Field<string>("Description"),
                    IsDefault = row.Field<bool>("IsDefault"),
                }).ToList();
            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            ViewBag.ProjectList = ProjectList();
            tblModuleViewModel viewModel = new tblModuleViewModel();
            viewModel.modules = tblModules;

            ViewData["PageId"] = "SA003";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(tblModuleViewModel viewModel)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            tblModule oModel = viewModel.module;
            oModel.CreatedOn = DateTime.Now;
            oModel.ActiveStatus = true;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.ModuleCheckDuplicacy(new tblModule { Id = oModel.Id, ModuleName = oModel.ModuleName, ProjectId = oModel.ProjectId }, "tDuplicate");
                    if (this.LocalDS.Tables["tDuplicate"] != null
                        && this.LocalDS.Tables["tDuplicate"].Rows.Count > 0)
                    {
                        TempData["error"] = $"Module Already exists";
                    }
                    else
                    {
                        oData.ModuleUpsert(oModel);
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

            return RedirectToAction("Index", "Module");
        }

        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.ModuleGetById(nId, "tModuleGetById");
                    var oModel = this.LocalDS.Tables["tModuleGetById"].AsEnumerable()
                        .Select(row => new tblModule
                        {
                            Id = row.Field<int>("Id"),
                            ProjectId = row.Field<int>("ProjectId"),
                            ModuleName = row.Field<string>("ModuleName"),
                            Description = row.Field<string>("Description"),
                            IsDefault = row.Field<bool>("IsDefault"),
                        }).FirstOrDefault();

                    oModel.ActiveStatus = false;
                    oData.ModuleUpsert(oModel);
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

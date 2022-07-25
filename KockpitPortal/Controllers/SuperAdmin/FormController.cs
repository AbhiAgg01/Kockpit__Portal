using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models;
using KockpitPortal.Models.SuperAdmin;
using KockpitPortal.ViewModels.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class FormController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;

        public FormController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
            _oData = oData;
            _LocalDS = LocalDS;
        }

        public IActionResult Index()
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            List<tblFormMaster> forms = new List<tblFormMaster>();
            try
            {
                oData.FormGetAll("tForms");
                if (this.LocalDS.Tables["tForms"] != null
                    && this.LocalDS.Tables["tForms"].Rows.Count > 0)
                {
                    forms = this.LocalDS.Tables["tForms"].AsEnumerable().Select(row =>
                    new tblFormMaster
                    {
                        Id = row.Field<int>("Id"),
                        ProjectId = row.Field<int>("ProjectId"),
                        ProjectName = row.Field<string>("ProjectName"),
                        CategoryId = row.Field<int>("CategoryId"),
                        CategoryName = row.Field<string>("CategoryName"),
                        FormName = row.Field<string>("FormName"),
                        Description = row.Field<string>("Description"),
                        ActionName = row.Field<string>("ActionName"),
                        ControllerName = row.Field<string>("ControllerName"),
                        ModuleName = string.Join("",row.Field<string>("ModuleName").Split(",").ToList().Select(c=> $"<span class='badge badge-primary'>{c}</span> &nbsp;").ToList()),
                        ModuleIds = row.Field<string>("ModuleIds"),
                        PageCode = row.Field<string>("PageCode"),
                        LinkIcon = row.Field<string>("LinkIcon"),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            tblFormViewModel formViewModel = new tblFormViewModel();
            formViewModel.forms = forms;

            ViewBag.ProjectList = ProjectList();
            ViewBag.ModuleList = ModuleList();
            ViewData["PageId"] = "SA005";
            return View(formViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblFormViewModel viewModel, int[] ModuleList)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            tblFormMaster oModel = viewModel.form;
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    List<int> listModules = new List<int>();
                    listModules = ModuleList.ToList();

                    //validate for duplicate username and schema
                    oData.FormCheckDuplicacy(oModel, "tDuplicateForm");
                    if (this.LocalDS.Tables["tDuplicateForm"] != null
                        && this.LocalDS.Tables["tDuplicateForm"].Rows.Count > 0)
                    {
                        TempData["error"] = $"Form Already exists";
                        return RedirectToAction("Index", "Form");
                    }

                    if (oModel.Id != 0)
                    {
                        oData.FormUpdate(oModel, listModules);
                        TempData["success"] = "Form Updated successfully";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        oModel.CreatedOn = DateTime.Now;
                        oModel.ActiveStatus = true;
                        oData.FormInsert(oModel, listModules);
                        TempData["success"] = "Form created successfully";
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

            return RedirectToAction("Index", "Form");
        }



        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res = oData.FormRemove(nId);
                    if (res)
                    {
                        TempData["success"] = "Form deleted successfully";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        TempData["error"] = $"Error while removing the form";
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
            return RedirectToAction("Index", "Form");
        }


        public JsonResult GetDataByProject(int nProjectId)
        {
            Response response = new Response();
            response.success = true;
            List<SelectListItem> listCategory = new List<SelectListItem>();
            List<SelectListItem> listModule = new List<SelectListItem>();
            try
            {
                oData.FormCategoryByProject(nProjectId, "tCategoryList");
                if(this.LocalDS.Tables["tCategoryList"] !=null && this.LocalDS.Tables["tCategoryList"].Rows.Count > 0)
                {
                    listCategory = this.LocalDS.Tables["tCategoryList"].AsEnumerable().Select(row => 
                    new SelectListItem { 
                        Text = row.Field<string>("CategoryName"),
                        Value = row.Field<int>("Id").ToString()
                    }).ToList();
                }

                oData.FormModuleByProject(nProjectId, "tModuleList");
                if (this.LocalDS.Tables["tModuleList"] != null && this.LocalDS.Tables["tModuleList"].Rows.Count > 0)
                {
                    listModule = this.LocalDS.Tables["tModuleList"].AsEnumerable().Select(row =>
                    new SelectListItem
                    {
                        Text = row.Field<string>("ModuleName"),
                        Value = row.Field<int>("Id").ToString()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.msg = $"Error : {ex.Message}";
            }
            return Json(new Tuple<Response, List<SelectListItem>, List<SelectListItem>>(response, listCategory, listModule));
        }
    }
}

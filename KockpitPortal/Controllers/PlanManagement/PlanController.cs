using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.Cart;
using KockpitPortal.Models.PlanManagement;
using KockpitPortal.ViewModels.PlanManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KockpitPortal.Controllers.PlanManagement
{
    public class PlanController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public PlanController(IConfiguration config, IHostingEnvironment env)
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
            List<tblCart> _carts = new List<tblCart>();
            List<tblPlan> tblPlans = new List<tblPlan>();
            oData.PlanGetAll("tPlanGetAll");
            if (this.LocalDS.Tables["tPlanGetAll"] != null
                && this.LocalDS.Tables["tPlanGetAll"].Rows.Count > 0)
            {
                tblPlans = this.LocalDS.Tables["tPlanGetAll"].AsEnumerable().Select(row =>
                new tblPlan
                {
                    Id = row.Field<int>("Id"),
                    PlanName = row.Field<string>("PlanName"),
                    Description = row.Field<string>("Description"),
                    ProjectId = row.Field<int>("ProjectId"),
                    ProjectName = row.Field<string>("ProjectName"),
                    NoOfUsers = row.Field<int>("NoOfUsers"),
                    ValidityDays = row.Field<int>("ValidityDays"),
                    IsFree = row.Field<bool>("IsFree"),
                    PlanPrice = row.Field<string>("PlanPrice"),
                    IsWarranty = row.Field<bool>("iswarranty"),
                    WarrantyInDays = row.Field<int?>("warrantyindays")
                }).ToList();
            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];

            ViewBag.ProjectList = ProjectList();
            tblPlanViewModel viewModel = new tblPlanViewModel();

            Dictionary<string, List<tblPlan>> data = tblPlans
                            .GroupBy(o => o.ProjectName)
                            .ToDictionary(g => g.Key, g => g.ToList());

            viewModel.data = data;
            viewModel.plans = tblPlans;

            oData.GetProductsFromCart(USERID, "tUpdatedCart");
            if (this.LocalDS.Tables["tProjects2"] != null && this.LocalDS.Tables["tProjects2"].Rows.Count > 0)
            {
                foreach (DataRow dr in this.LocalDS.Tables["tProjects2"].Rows)
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
            ViewData["PageId"] = "SA008";
            var list = ProjectList();
            list.RemoveAt(0);
            list.Insert(0, new SelectListItem { Text = "All", Value = "All" });

            ViewBag.FilterProject = list;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(tblPlanViewModel viewModel)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            string strFilePath = string.Empty;
            string strVideoFilePath = string.Empty;
            tblPlan oModel = viewModel.plan;
            oModel.CreatedOn = DateTime.Now;
            oModel.ActiveStatus = true;
            oModel.NoOfUsers = 0;

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.PlanCheckDuplicacy(new tblPlan { Id = oModel.Id, PlanName = oModel.PlanName }, "tDuplicate");
                    if (this.LocalDS.Tables["tDuplicate"] != null
                        && this.LocalDS.Tables["tDuplicate"].Rows.Count > 0)
                    {
                        TempData["error"] = $"Plan Already exists";
                    }
                    else
                    {
                        //if (oModel.Id != 0)
                        //{
                        //    oData.ProjectGetById(oModel.Id, "tProjectGetById");
                        //    if (this.LocalDS.Tables["tProjectGetById"] != null && this.LocalDS.Tables["tProjectGetById"].Rows.Count > 0)
                        //    {
                        //        oModel.ProjectImage = this.LocalDS.Tables["tProjectGetById"].Rows[0]["ProjectImage"].ToString().Trim();
                        //        oModel.ProjectVideo = this.LocalDS.Tables["tProjectGetById"].Rows[0]["ProjectVideo"].ToString().Trim();
                        //    }
                        //}
                        oData.PlanUpsert(oModel);
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

            return RedirectToAction("Index", "Plan");
        }

        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.PlanGetById(nId.ToString(), "tPlanGetById");
                    var oModel = this.LocalDS.Tables["tPlanGetById"].AsEnumerable()
                        .Select(row => new tblPlan
                        {
                            Id = row.Field<int>("Id"),
                            PlanName = row.Field<string>("PlanName"),
                            Description = row.Field<string>("Description"),
                            ProjectId = row.Field<int>("ProjectId"),
                            NoOfUsers = row.Field<int>("NoOfUsers"),
                            ValidityDays = row.Field<int>("ValidityDays"),
                            IsFree = row.Field<bool>("IsFree"),
                            PlanPrice = row.Field<string>("PlanPrice"),
                        }).FirstOrDefault();
                    oModel.ActiveStatus = false;
                    oData.PlanUpsert(oModel);
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
            return RedirectToAction("Index", "Plan");
        }
    }
}

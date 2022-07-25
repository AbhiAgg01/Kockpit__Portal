using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    public class ProjectController : BaseController
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public ProjectController(IConfiguration config, IHostingEnvironment env)
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

            List<tblProject> tblProjects = new List<tblProject>();
            oData.ProjectGetAll("tProjectGetAll");
            if (this.LocalDS.Tables["tProjectGetAll"] != null
                && this.LocalDS.Tables["tProjectGetAll"].Rows.Count > 0)
            {
                tblProjects = this.LocalDS.Tables["tProjectGetAll"].AsEnumerable().Select(row =>
                new tblProject
                {
                    Id = row.Field<int>("Id"),
                    OfferingCategory = row.Field<string>("OfferingCategory"),
                    ProjectName = row.Field<string>("ProjectName"),
                    Description = row.Field<string>("Description"),
                    Version = row.Field<string>("Version"),
                    ProjectType = row.Field<string>("ProjectType"),
                    ProjectImage = Url.Content(row.Field<string>("ProjectImage")),
                    ProjectVideo = Url.Content(row.Field<string>("ProjectVideo")),
                    ProjectStartUpLink = row.Field<string>("ProjectStartUpLink"),
                    IsChat = row.Field<bool>("IsChat"),
                    IsPro = row.Field<bool>("IsPro"),
                    ProjectIcon = Url.Content(row.Field<string>("ProjectIcon")),
                }).ToList();
            }

            ViewBag.Category = ProjectCategoryList();

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            tblProjectViewModel viewModel = new tblProjectViewModel();
            viewModel.projects = tblProjects;
            ViewData["PageId"] = "SA002";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(tblProjectViewModel viewModel)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            string strFilePath = string.Empty;
            string strVideoFilePath = string.Empty;
            string strIconFilePath = string.Empty;
            tblProject oModel = viewModel.project;
            oModel.CreatedOn = DateTime.Now;
            oModel.ActiveStatus = true;
            oModel.ProjectImage = string.Empty;
            oModel.ProjectVideo = string.Empty;

            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.ProjectCheckDuplicacy(new tblProject { Id = oModel.Id, ProjectName = oModel.ProjectName }, "tDuplicate");
                    if (this.LocalDS.Tables["tDuplicate"] != null
                        && this.LocalDS.Tables["tDuplicate"].Rows.Count > 0)
                    {
                        TempData["error"] = $"Project Already exists";
                    }
                    else
                    {
                        if (oModel.Id != 0)
                        {
                            oData.ProjectGetById(oModel.Id, "tProjectGetById");
                            if (this.LocalDS.Tables["tProjectGetById"] != null && this.LocalDS.Tables["tProjectGetById"].Rows.Count > 0)
                            {
                                oModel.ProjectImage = this.LocalDS.Tables["tProjectGetById"].Rows[0]["ProjectImage"].ToString().Trim();
                                oModel.ProjectVideo = this.LocalDS.Tables["tProjectGetById"].Rows[0]["ProjectVideo"].ToString().Trim();
                                oModel.ProjectIcon = this.LocalDS.Tables["tProjectGetById"].Rows[0]["ProjectIcon"].ToString().Trim();
                            }
                        }

                        if (oModel.file != null && oModel.file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(oModel.file.FileName);
                            strFilePath = Path.Combine(_env.WebRootPath, "Resource/Project", fileName);
                            if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                            }
                            using (Stream fileStream = new FileStream(strFilePath, FileMode.Create))
                            {
                                oModel.file.CopyTo(fileStream);
                            }

                            oModel.ProjectImage = Path.Combine("~/Resource/Project/", fileName);
                        }

                        if (oModel.Videofile != null && oModel.Videofile.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(oModel.Videofile.FileName);
                            strVideoFilePath = Path.Combine(_env.WebRootPath, "Resource/Project", fileName);
                            if (!Directory.Exists(Path.GetDirectoryName(strVideoFilePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(strVideoFilePath));
                            }
                            using (Stream fileStream = new FileStream(strVideoFilePath, FileMode.Create))
                            {
                                oModel.Videofile.CopyTo(fileStream);
                            }

                            oModel.ProjectVideo = Path.Combine("~/Resource/Project/", fileName);
                        }
                        if (oModel.IconFile != null && oModel.IconFile.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(oModel.IconFile.FileName);
                            strIconFilePath = Path.Combine(_env.WebRootPath, "Resource/Project", fileName);
                            if (!Directory.Exists(Path.GetDirectoryName(strIconFilePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(strIconFilePath));
                            }
                            using (Stream fileStream = new FileStream(strIconFilePath, FileMode.Create))
                            {
                                oModel.IconFile.CopyTo(fileStream);
                            }
                            oModel.ProjectIcon = Path.Combine("~/Resource/Project/", fileName);
                        }

                        oModel.IsPro = false;
                        oData.ProjectUpsert(oModel);
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

            return RedirectToAction("Index", "Project");
        }

        public IActionResult Remove(int nId)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    oData.ProjectGetById(nId, "tProjectGetById");
                    var oModel = this.LocalDS.Tables["tProjectGetById"].AsEnumerable()
                        .Select(row => new tblProject
                        {
                            Id = row.Field<int>("Id"),
                            OfferingCategory = row.Field<string>("OfferingCategory"),
                            ProjectName = row.Field<string>("ProjectName"),
                            Description = row.Field<string>("Description"),
                            Version = row.Field<string>("Version"),
                            ProjectType = row.Field<string>("ProjectType"),
                            ProjectImage = row.Field<string>("ProjectImage"),
                            ProjectVideo = row.Field<string>("ProjectVideo"),
                            ProjectStartUpLink = row.Field<string>("ProjectStartUpLink"),
                            IsChat = row.Field<bool>("IsChat"),
                            IsPro = row.Field<bool>("IsPro"),
                        }).FirstOrDefault();
                    oModel.ActiveStatus = false;
                    oData.ProjectUpsert(oModel);
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
            return RedirectToAction("Index", "Project");
        }
    }
}

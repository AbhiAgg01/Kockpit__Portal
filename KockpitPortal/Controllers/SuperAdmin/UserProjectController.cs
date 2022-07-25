using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.SuperAdmin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace KockpitPortal.Controllers.SuperAdmin
{
    public class UserProjectController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        public UserProjectController(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
        }

        public IActionResult Index()
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");

            Dictionary<tblUsers, List<tblProject>> diResult = new Dictionary<tblUsers, List<tblProject>>();
            oDataAuthenticator.UserProjects("tUserProject", "ADMIN");
            if (this.LocalDS.Tables["tUserProject"] != null && this.LocalDS.Tables["tUserProject"].Rows.Count > 0)
            {
                foreach(DataRow dr in this.LocalDS.Tables["tUserProject"].Rows)
                {
                    List<tblProject> projects = new List<tblProject>();
                    var user = new tblUsers
                    {
                        Id = Convert.ToInt32(dr["Id"].ToString()),
                        CompanyName = dr["CompanyName"].ToString(),
                        Logo = dr["Logo"].ToString()
                    };
                    if (!diResult.Keys.Contains(user))
                    {
                        diResult[user] = projects;
                    }
                    //foreach (var proj in this.LocalDS.Tables["tUserProject"].AsEnumerable().Where(c => c.Field<int>("Id") == user.Id).ToList())
                    //{
                    //    if (!string.IsNullOrEmpty(proj.Field<string>("ProjectName").ToString()))
                    //    {
                    //        var tblProject = new tblProject
                    //        {
                    //            ProjectName = proj.Field<string>("ProjectName").ToString()
                    //        };
                    //        if (!projects.Contains(tblProject))
                    //        {
                    //            projects.Add(tblProject);
                    //        }
                    //    }
                    //}
                }

                //var result = from rows in this.LocalDS.Tables["tUserProject"].AsEnumerable()
                //             group rows by new { Id = rows["Id"], CompanyName = rows["CompanyName"], Logo = rows["Logo"]  } into grp
                //             select grp;
                //if (result != null && result.Count() > 0)
                //{

                //    //foreach(var item in result)
                //    //{
                //    //    diResult.Add(
                //    //        new tblUsers { Id = (int)item.Key.Id, CompanyName = (string)item.Key.CompanyName, Logo = (string)item.Key.Logo },
                //    //        new List<tblProject> { new tblProject { item  } }
                //    //        );
                //    //}
                //}
            }
            ViewBag.Data = diResult;
            return View();
        }
    }
}

using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.Support;
using KockpitPortal.ViewModels.Support;
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
    public class TicketIssueController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public TicketIssueController(IConfiguration config, IHostingEnvironment env)
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
            var issues = new List<tblTicketIssue>();
            TblTicketIssueViewModal tblTicketIssueViewModal = new TblTicketIssueViewModal();
            oDataAuthenticator.GetTicketIssue("tTicketIssue");
            if (this.LocalDS.Tables["tTicketIssue"] != null
                && this.LocalDS.Tables["tTicketIssue"].Rows.Count > 0)
            {
                issues = this.LocalDS.Tables["tTicketIssue"].AsEnumerable().Select(rows =>
                new tblTicketIssue
                {
                    Id = rows.Field<int>("id"),
                    Description = rows.Field<string>("discription"),
                    Issue = rows.Field<string>("issuetype"),
                    ActiveStatus = rows.Field<bool>("activestatus"),
                    CreatedOn = rows.Field<DateTime>("createdon")
                }).ToList();
            }
            tblTicketIssueViewModal.tblTicketIssues = issues;
            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageTitle"] = "Ticket Issue";
            ViewData["PageId"] = "SA011";
            return View(tblTicketIssueViewModal);
        }

        [HttpPost]
        public IActionResult Upsert(tblTicketIssue tblTicketIssue)
        {
            if (!ISSUPERADMIN) return RedirectToAction("Logout", "Login");
            using (TransactionScope transaction =
                new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                   var success =  oDataAuthenticator.TicketIssueUpsert(tblTicketIssue);
                    if (success)
                    {
                        if(tblTicketIssue.Id != 0)
                            TempData["success"] = "Updated successfully";
                        else
                            TempData["success"] = "Created successfully";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                }
                catch(TransactionException ex)
                {
                    TempData["error"] = $"Error : {ex.Message}";
                    transaction.Dispose();
                }
                catch(Exception ex)
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
                    oDataAuthenticator.DeleteTicketIssue(nId);  
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

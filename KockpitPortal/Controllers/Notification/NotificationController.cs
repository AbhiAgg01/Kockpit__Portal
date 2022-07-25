using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.Notification;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace KockpitPortal.Controllers.Notification
{
    public class NotificationController : BaseController
    {
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();
        private IHostingEnvironment _env;
        IConfiguration _config;
        public NotificationController(IConfiguration config, IHostingEnvironment env)
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
            List<tblNotification> _notifications = new List<tblNotification>();
            oDataAuthenticator.GetNotification(USERID, "tNotifications");
            if (this.LocalDS.Tables["tNotifications"] != null
                && this.LocalDS.Tables["tNotifications"].Rows.Count > 0)
            {
                _notifications = this._LocalDS.Tables["tNotifications"].AsEnumerable().Select(row =>
                new tblNotification
                {
                    Id = row.Field<int>("Id"),
                    Subject = row.Field<string>("Subject"),
                    Message = row.Field<string>("Message"),
                    Createdon = row.Field<DateTime>("createdon"),
                    IsRead = row.Field<bool>("isread"),
                }).ToList();

                ViewBag.Notifications = _notifications;
            }
            else
            {
                ViewBag.Notifications = null;
            }

            ViewData["success"] = TempData["success"];
            ViewData["error"] = TempData["error"];
            ViewData["PageId"] = "NTF-T-01";
            ViewData["PageTitle"] = "Notifications";
            return View();
        }

        public IActionResult DeleteNotificaion(int id)
        {
            using (TransactionScope transaction =
              new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res = oDataAuthenticator.DeleteNotification(id);
                    if (res)
                    {
                        TempData["success"] = "Succesfully Deleted Notification";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        TempData["error"] = "Can't delete Notification";
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
            }
            return RedirectToAction("Index", "Notification");
        }

        public IActionResult DeleteAllNotificaion()
        {
            using (TransactionScope transaction =
               new TransactionScope(TransactionScopeOption.RequiresNew, System.TimeSpan.MaxValue))
            {
                try
                {
                    var res = oDataAuthenticator.DeleteNotifications(USERID);
                    if (res)
                    {
                        TempData["success"] = "Succesfully Deleted Notification";
                        transaction.Complete();
                        transaction.Dispose();
                    }
                    else
                    {
                        TempData["error"] = "Can't delete Notification";
                        transaction.Dispose();
                    }
                }
                catch (TransactionException e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
                catch (Exception e)
                {
                    TempData["error"] = e.Message;
                    transaction.Dispose();
                }
            }
            return RedirectToAction("Index", "Notification");
        }
    }
}


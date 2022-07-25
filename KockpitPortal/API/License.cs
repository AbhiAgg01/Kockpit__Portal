using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.PlanManagement;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KockpitPortal.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class License : ControllerBase
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        IConfiguration _baseConfig;
        private IHostingEnvironment _env;
        public License(IConfiguration config, IHostingEnvironment env)
        {
            _baseConfig = config;
            oData = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());
            _env = env;
        }

        public string KockpitAuthenticatorConnection(string strDatabase = "")
        {
            PostgreConnection postgreConnection = new PostgreConnection();
            postgreConnection.Host = _baseConfig["KockpitAuthenticatorPsql:Host"] != null ? _baseConfig["KockpitAuthenticatorPsql:Host"].ToString() : "";
            postgreConnection.Port = _baseConfig["KockpitAuthenticatorPsql:Port"] != null ? _baseConfig["KockpitAuthenticatorPsql:Port"].ToString() : "";
            postgreConnection.Username = _baseConfig["KockpitAuthenticatorPsql:Username"] != null ? _baseConfig["KockpitAuthenticatorPsql:Username"].ToString() : "";
            postgreConnection.Password = _baseConfig["KockpitAuthenticatorPsql:Password"] != null ? _baseConfig["KockpitAuthenticatorPsql:Password"].ToString() : "";
            postgreConnection.Database = string.IsNullOrEmpty(strDatabase)
                ? _baseConfig["KockpitAuthenticatorPsql:Database"] != null ? _baseConfig["KockpitAuthenticatorPsql:Database"].ToString() : ""
                : strDatabase;
            return string.IsNullOrEmpty(postgreConnection.Port)
                ? $"Host={postgreConnection.Host};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}"
                : $"Host={postgreConnection.Host};Port={postgreConnection.Port};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}";
        }


        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LicenseAuth user)
        {
            try
            {
                oData.AuthenticateLicense(user.Username, user.License, "tAuthenticate");
                if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                {
                    var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
                    if (expiryDate < DateTime.Now)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, "License Expired");
                    }
                    else
                    {
                        var _logo = Url.Content(this.LocalDS.Tables["tAuthenticate"].Rows[0]["Logo"].ToString().Trim());
                        var _company = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ParentCompany"].ToString().Trim();
                        var _socketId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["SocketId"].ToString().Trim();
                        var _companyId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["CompanyId"].ToString().Trim();
                        var _companyName = this.LocalDS.Tables["tAuthenticate"].Rows[0]["CompanyName"].ToString().Trim();
                        var _expiryDate = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString().Trim();
                        var _userid = this.LocalDS.Tables["tAuthenticate"].Rows[0]["UserId"].ToString().Trim();
                        var _role = this.LocalDS.Tables["tAuthenticate"].Rows[0]["Role"].ToString().Trim();

                        double nDaysLeft = 0;
                        if(_expiryDate != null && DateTime.TryParse(_expiryDate, out DateTime dtExpiryDate))
                        {
                            var currentDate = DateTime.Now.Date;
                            nDaysLeft = (dtExpiryDate - currentDate).TotalDays;
                        }

                        //
                        //check for device id
                        var ExisitingDeviceId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["DeviceId"].ToString().Trim();
                        if (string.IsNullOrEmpty(ExisitingDeviceId))
                        {
                            oData.UpdateLicenseDeviceId(user.License, user.Device);
                            var obj = new
                            {
                                companyid = Convert.ToInt32(_companyId),
                                message = "valid",
                                logo = _logo,
                                company = _company,
                                socketid = _socketId,
                                name = _companyName,
                                daysleft = nDaysLeft,
                                userid = _userid,
                                role = _role
                            };
                            return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(obj).ToString().Trim());
                        }
                        else
                        {
                            if (ExisitingDeviceId != user.Device)
                                return StatusCode((int)HttpStatusCode.InternalServerError, "Already running on another device ! Please logout from active device.");
                            else
                            {
                                var obj = new
                                {
                                    companyid = Convert.ToInt32(_companyId),
                                    message = "valid",
                                    logo = _logo,
                                    company = _company,
                                    socketid = _socketId,
                                    name = _companyName,
                                    daysleft = nDaysLeft,
                                    userid = _userid,
                                    role = _role
                                };
                                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(obj).ToString().Trim());
                            }
                        }
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Created, "No License key found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }


        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] LicenseAuth user)
        {
            try
            {
                oData.AuthenticateLicense(user.Username, user.License, "tAuthenticate");
                if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                {
                    var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
                    if (expiryDate < DateTime.Now)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, "License Expired");
                    }
                    else
                    {
                        var res = oData.RemoveLicenseDeviceId(user.Username, user.License);
                        if (res)
                        {
                            return StatusCode((int)HttpStatusCode.OK, "Success");
                        }
                        else
                        {
                            return StatusCode((int)HttpStatusCode.InternalServerError, "Error");
                        }
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Created, "No License key found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        //#region [App]
        //[HttpPost("app/checkdomain")]
        //public async Task<IActionResult> AuthenticateDomain(string domain)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(domain))
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, "Invalid input");
        //        }

        //        oData.AuthenticateCompanyDomain(domain, "tAuthenticateDomain");
        //        if (this.LocalDS.Tables["tAuthenticateDomain"] != null && this.LocalDS.Tables["tAuthenticateDomain"].Rows.Count > 0)
        //            return StatusCode((int)HttpStatusCode.OK, "Exists");
        //        else
        //            return StatusCode((int)HttpStatusCode.NotFound, "Not found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError, $"Error : {ex.Message}");
        //    }
        //}
        //[HttpPost("app/authenticate")]
        //public async Task<IActionResult> AuthenticateApp([FromBody] LicenseAuth user)
        //{
        //    try
        //    {
        //        if(user == null)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, "Invalid input");
        //        }

        //        oData.AuthenticateLicense(user.Username, user.License, "tAuthenticate");
        //        if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
        //        {
        //            var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
        //            if (expiryDate < DateTime.Now)
        //                return StatusCode((int)HttpStatusCode.InternalServerError, "License Expired");
        //            else
        //            {
        //                oData.MobileAppDeviceGetByUser(user.Username, user.License, "tMobileAppDeviceGetByUser");
        //                int nCount = 0;
        //                if (this.LocalDS.Tables["tMobileAppDeviceGetByUser"] != null
        //                    && this.LocalDS.Tables["tMobileAppDeviceGetByUser"].Rows.Count > 0)
        //                {
        //                    nCount = this.LocalDS.Tables["tMobileAppDeviceGetByUser"].Rows.Count;
        //                }

        //                if (nCount > 3)
        //                    return StatusCode((int)HttpStatusCode.InternalServerError, "Already exceed the limit upto 3 devices, Please remove devices");
        //                else
        //                {
        //                    oData.MobileAppDeviceGetByUserLicense(user.Username, user.License, user.Device, "tMobileAppDeviceGetByUserLicense");
        //                    if (this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"] != null
        //                        && this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"].Rows.Count > 0) { }
        //                    else
        //                    {
        //                        oData.MobileAppDeviceInsert(user.Username, user.License, user.Device, user.Domain, user.OfferingCategory);
        //                    }
        //                }

        //                var _logo = Url.Content(this.LocalDS.Tables["tAuthenticate"].Rows[0]["Logo"].ToString().Trim());
        //                var _company = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ParentCompany"].ToString().Trim();
        //                var _socketId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["SocketId"].ToString().Trim();
        //                var _companyId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["CompanyId"].ToString().Trim();
        //                var _EmpCode = this.LocalDS.Tables["tAuthenticate"].Rows[0]["EmpCode"].ToString().Trim();
        //                var _ContactNo2 = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ContactNo2"].ToString().Trim();
        //                var _ContactNo1 = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ContactNo1"].ToString().Trim();
        //                var obj = new
        //                {
        //                    companyid = Convert.ToInt32(_companyId),
        //                    message = "valid",
        //                    logo = _logo,
        //                    company = _company,
        //                    socketid = _socketId,
        //                    UserId  = _EmpCode,
        //                    PhoneNo = _ContactNo2,
        //                    whatsappno = _ContactNo1
        //                };
        //                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(obj).ToString().Trim());
        //            }
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.NotFound, "No License key found");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error : {ex.Message}");
        //    }
        //}

        //[HttpPost("app/remove")]
        //public async Task<IActionResult> RemoveApp([FromBody] LicenseAuth user)
        //{
        //    try
        //    {
        //        oData.AuthenticateLicense(user.Username, user.License, "tAuthenticate");
        //        if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
        //        {
        //            var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
        //            if (expiryDate < DateTime.Now)
        //                return StatusCode((int)HttpStatusCode.InternalServerError, "License Expired");
        //            else
        //            {
        //                var res = oData.MobileAppDeviceRemoveDeviceId(user.Username, user.License, user.Device);
        //                if (res)
        //                    return StatusCode((int)HttpStatusCode.OK, "Success");
        //                else
        //                    return StatusCode((int)HttpStatusCode.InternalServerError, "Error");
        //            }
        //        }
        //        else
        //            return StatusCode((int)HttpStatusCode.NotFound, "No License key found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error : {ex.Message}");
        //    }
        //}

        //[HttpPost("app/devices")]
        //public async Task<IActionResult> DeviceApp([FromBody] LicenseAuth user)
        //{
        //    try
        //    {
        //        oData.MobileAppDeviceGetAllDevice(user.Username, user.License, "tMobileAppDeviceGetAllDevice");
        //        if (this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"] != null
        //            && this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"].Rows.Count > 0)
        //        {
        //            var objDevices = this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"].AsEnumerable()
        //                .Select(row => new tblMobileAppDevice
        //                {
        //                    id = row.Field<int>("id"),
        //                    Username = row.Field<string>("Username"),
        //                    License = row.Field<string>("License"),
        //                    Device = row.Field<string>("Device"),
        //                    IsAllowed = row.Field<bool>("IsAllowed"),
        //                }).ToList();
        //            return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objDevices));
        //        }
        //        else
        //            return StatusCode((int)HttpStatusCode.NotFound, "No Records found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error : {ex.Message}");
        //    }
        //}

        //[HttpPost("app/validate")]
        //public async Task<IActionResult> DeviceValidate([FromBody] LicenseAuth user)
        //{
        //    try
        //    {
        //        oData.MobileAppDeviceGetByUserLicense(user.Username, user.License, user.Device, "tMobileAppDeviceGetByUserLicense");
        //        if (this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"] != null
        //            && this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"].Rows.Count > 0) 
        //        {
        //            var isAllowed = Convert.ToBoolean(this.LocalDS.Tables["MobileAppDeviceGetByUserLicense"].Rows[0]["IsAllowed"].ToString().Trim());
        //            return StatusCode((int)HttpStatusCode.OK, isAllowed);
        //        }
        //        else
        //            return StatusCode((int)HttpStatusCode.NotFound, "No Records found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error : {ex.Message}");
        //    }
        //}
        //#endregion
    }

    public class LicenseAuth
    {
        public string Username { get; set; }
        public string License { get; set; }
        public string Device { get; set; }
        public string Domain { get; set; }
        public string OfferingCategory { get; set; }
    }
}

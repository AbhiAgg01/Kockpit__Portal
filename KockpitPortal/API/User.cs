using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KockpitPortal.Controllers;
using KockpitPortal.DataAccessLayer;
using KockpitPortal.Models.PlanManagement;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KockpitPortal.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class User : ControllerBase
    {
        DataPostgres oData;
        DataSet LocalDS = new DataSet();
        IConfiguration _baseConfig;
        private IHostingEnvironment _env;
        public User(IConfiguration config, IHostingEnvironment env)
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
        public async Task<IActionResult> Authenticate([FromBody] UserAuth user)
        {
            try
            {
                oData.AuthenticateLogin(user.Username, Secure.Encryptdata(user.Password), "tAuthenticate");
                if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, "Valid");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Invalid Username & Password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetAll(int id)
        {
            try
            {
                oData.SubUserGetAll(id, "tUserList");
                if (this.LocalDS.Tables["tUserList"] != null && this.LocalDS.Tables["tUserList"].Rows.Count > 0)
                {
                    var objUserList = new List<UserList>();
                    objUserList = this.LocalDS.Tables["tUserList"].AsEnumerable().Select(row =>
                    new UserList { 
                        id = row.Field<int>("Id"),
                        Email = row.Field<string>("EmailId"),
                        Name = row.Field<string>("CompanyName"),
                        EmpCode = row.Field<string>("EmpCode"),
                    }).ToList();

                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objUserList));
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Invalid Username & Password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("getallclient")]
        public async Task<IActionResult> GetAllClient()
        {
            try
            {
                oData.ClientUserGetAll("tUserList");
                if (this.LocalDS.Tables["tUserList"] != null && this.LocalDS.Tables["tUserList"].Rows.Count > 0)
                {
                    var objUserList = new List<UserList>();
                    objUserList = this.LocalDS.Tables["tUserList"].AsEnumerable().Select(row =>
                    new UserList { 
                        id = row.Field<int>("Id"),
                        Email = row.Field<string>("EmailId"),
                        Name = row.Field<string>("CompanyName"),
                        EmpCode = row.Field<string>("EmpCode"),
                        CompanyDomain = row.Field<string>("SchemaName"),
                    }).ToList();

                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objUserList));
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NoContent, "No Users Found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("checkaccessid")]
        public async Task<IActionResult> AuthenticateAccessId([FromBody] AcessIdAuth acessIdAuth)
        {
            Response response = new Response();
            try
            {
                oData.CheckAccessIdExits(acessIdAuth.EmpCode, acessIdAuth.Domain, "tCheckAcessId");
                if (this.LocalDS.Tables["tCheckAcessId"] != null && this.LocalDS.Tables["tCheckAcessId"].Rows.Count > 0)
                {
                    response.message = $"AccessId:{acessIdAuth.EmpCode} Exits";
                    response.status = true;
                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(response).ToString().Trim());
                }
                else
                {
                    response.message = "AccessId NotFound";
                    response.status = false;
                    return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(response).ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("activedevice")]
        public async Task<IActionResult> GetActiveDevice([FromBody] ActiveDevice activeDevice)
        {
            try
            {
                oData.PlanPurchaseGetActiveDeviceOnLicense(activeDevice.UserId, activeDevice.LicenseKey, "tPlanPurchaseGetActiveDeviceOnLicense");
                if (this.LocalDS.Tables["tPlanPurchaseGetActiveDeviceOnLicense"] != null 
                    && this.LocalDS.Tables["tPlanPurchaseGetActiveDeviceOnLicense"].Rows.Count > 0)
                {
                    var activeDeviceName = this.LocalDS.Tables["tPlanPurchaseGetActiveDeviceOnLicense"].Rows[0]["DeviceId"].ToString().Trim();
                    return StatusCode((int)HttpStatusCode.OK, activeDeviceName);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "No active device found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error : " + ex.Message);
            }
        }


        #region [App]
        [HttpPost("app/authenticate")]
        public async Task<IActionResult> AuthenticateApp([FromBody] UserAuth user)
        {
            MobileAppResponse mobileAppResponse = new MobileAppResponse();
            mobileAppResponse.UserValid = false;
            mobileAppResponse.LicenseValid = false;
            try
            {
                oData.AuthenticateLogin(user.Username, Secure.Encryptdata(user.Password), "tAuthenticate", user.Domain);
                if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                {
                    mobileAppResponse.UserValid = true;
                    //checking for license
                    var nUserId = Convert.ToInt32(this.LocalDS.Tables["tAuthenticate"].Rows[0]["Id"].ToString());
                    var licenseKey = oData.GetLicenseKeyByUserId(nUserId, user.OfferingCategory);
                    if (string.IsNullOrEmpty(licenseKey))
                    {
                        mobileAppResponse.msg = "License key not found.";
                        return StatusCode((int)HttpStatusCode.NoContent, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                    }
                    else
                    {
                        oData.AuthenticateLicense(user.Username, licenseKey, "tAuthenticate");
                        if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                        {
                            var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
                            if (expiryDate < DateTime.Now)
                                return StatusCode((int)HttpStatusCode.NonAuthoritativeInformation, "License Expired");
                            else
                            {
                                mobileAppResponse.LicenseValid = true;

                                oData.MobileAppDeviceGetByUser(user.Username, licenseKey, "tMobileAppDeviceGetByUser");
                                bool lContinue = true;

                                int nCount = 0;

                                if (this.LocalDS.Tables["tMobileAppDeviceGetByUser"] != null
                                    && this.LocalDS.Tables["tMobileAppDeviceGetByUser"].Rows.Count > 0)
                                {
                                    var vtblMobileAppDevice = this.LocalDS.Tables["tMobileAppDeviceGetByUser"]
                                        .AsEnumerable()
                                        .Select(row => new tblMobileAppDevice
                                        {
                                            Username = row.Field<string>("Username"),
                                            License = row.Field<string>("License"),
                                            Device = row.Field<string>("Device"),
                                            Domain = row.Field<string>("Domain"),
                                            IsAllowed = row.Field<bool>("IsAllowed")
                                        }).ToList();

                                    var IsExistingDevice = vtblMobileAppDevice.Where(c => c.Device == user.Device && c.IsAllowed == true)
                                        .Any();

                                    lContinue = IsExistingDevice;
                                    nCount = this.LocalDS.Tables["tMobileAppDeviceGetByUser"].Rows.Count;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(user.Device))
                                    {
                                        oData.MobileAppDeviceInsert(user.Username, licenseKey, user.Device, user.Domain, user.OfferingCategory);
                                    }
                                    else
                                    {
                                        mobileAppResponse.msg = "Device Id Required.";
                                        return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                                    }
                                }
                              
                                if (!lContinue)
                                {
                                    if (nCount >= 3)
                                        return StatusCode((int)HttpStatusCode.NonAuthoritativeInformation, "Already exceed the limit upto 3 devices, Please remove devices");
                                    else
                                        if (!string.IsNullOrEmpty(user.Device))
                                    {
                                        oData.MobileAppDeviceInsert(user.Username, licenseKey, user.Device, user.Domain, user.OfferingCategory);
                                    }
                                    else
                                    {
                                        mobileAppResponse.msg = "Device Id Required.";
                                        return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                                    }
                                }

                                var _logo = Url.Content(this.LocalDS.Tables["tAuthenticate"].Rows[0]["Logo"].ToString().Trim());
                                var _company = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ParentCompany"].ToString().Trim();
                                var _socketId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["SocketId"].ToString().Trim();
                                var _companyId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["CompanyId"].ToString().Trim();
                                var _EmpCode = this.LocalDS.Tables["tAuthenticate"].Rows[0]["EmpCode"].ToString().Trim();
                                var _ContactNo2 = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ContactNo2"].ToString().Trim();
                                var _ContactNo1 = this.LocalDS.Tables["tAuthenticate"].Rows[0]["ContactNo1"].ToString().Trim();
                                var _Domain = this.LocalDS.Tables["tAuthenticate"].Rows[0]["SchemaName"].ToString().Trim();
                                mobileAppResponse.Companyid = Convert.ToInt32(_companyId);
                                mobileAppResponse.Logo = _logo;
                                mobileAppResponse.Company = _company;
                                mobileAppResponse.Socketid = _socketId;
                                mobileAppResponse.UserId = _EmpCode;
                                mobileAppResponse.PhoneNo = _ContactNo2;
                                mobileAppResponse.whatsappno = _ContactNo1;
                                mobileAppResponse.Domain = _Domain;
                                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                            }
                        }
                        else
                        {
                            mobileAppResponse.msg = "License key not found.";
                            return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                        }
                    }
                }
                else
                {
                    mobileAppResponse.msg = "Invalid username and password";
                    return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(mobileAppResponse).ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/checkdomainbyuser")]
        public async Task<IActionResult> CheckDomainApp([FromBody] UserDomain user)
        {
            try
            {
                if(user == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid Parameters");
                }

                oData.CheckDomainByUser(user.Username, user.Domain, "tCheckDomainByUser");
                if (this.LocalDS.Tables["tCheckDomainByUser"] != null 
                    && this.LocalDS.Tables["tCheckDomainByUser"].Rows.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, "Valid");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Invalid");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/checkdomain")]
        public async Task<IActionResult> AuthenticateDomain(string domain)
        {
            try
            {
                if (string.IsNullOrEmpty(domain))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid input");
                }

                oData.AuthenticateCompanyDomain(domain, "tAuthenticateDomain");
                if (this.LocalDS.Tables["tAuthenticateDomain"] != null && this.LocalDS.Tables["tAuthenticateDomain"].Rows.Count > 0)
                    return StatusCode((int)HttpStatusCode.OK, "Exists");
                else
                    return StatusCode((int)HttpStatusCode.Created, "Not found");
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/remove/all")]
        public async Task<IActionResult> RemoveAppDeviceAll([FromBody] UserDomain user)
        {
            try
            {
                if (user == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid Parameters");
                }
                oData.CheckDomainByUser(user.Username, user.Domain, "tCheckDomainByUser");
                if (this.LocalDS.Tables["tCheckDomainByUser"] != null
                    && this.LocalDS.Tables["tCheckDomainByUser"].Rows.Count > 0)
                {
                    var res = oData.MobileAppDeviceRemoveDeviceAll(user.Username, user.Domain, user.OfferingCategory);
                    if (res)
                        return StatusCode((int)HttpStatusCode.OK, "Success");
                    else
                        return StatusCode((int)HttpStatusCode.BadRequest, "Error");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }
        [HttpPost("app/remove")]
        public async Task<IActionResult> RemoveAppDevice([FromBody] UserDomain user)
        {
            try
            {
                if (user == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid Parameters");
                }
                oData.CheckDomainByUser(user.Username, user.Domain, "tCheckDomainByUser");
                if (this.LocalDS.Tables["tCheckDomainByUser"] != null
                    && this.LocalDS.Tables["tCheckDomainByUser"].Rows.Count > 0)
                {
                    var res = oData.MobileAppDeviceRemoveDevice(user.Username, user.Device, user.Domain, user.OfferingCategory);
                    if (res)
                        return StatusCode((int)HttpStatusCode.OK, "Success");
                    else
                        return StatusCode((int)HttpStatusCode.BadRequest, "Error");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/devices")]
        public async Task<IActionResult> DeviceApp([FromBody] LicenseAuth user)
        {
            try
            {
                oData.MobileAppDeviceGetAllDevice(user.Username, user.License, "tMobileAppDeviceGetAllDevice");
                if (this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"] != null
                    && this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"].Rows.Count > 0)
                {
                    var objDevices = this.LocalDS.Tables["tMobileAppDeviceGetAllDevice"].AsEnumerable()
                        .Select(row => new tblMobileAppDevice
                        {
                            id = row.Field<int>("id"),
                            Username = row.Field<string>("Username"),
                            License = row.Field<string>("License"),
                            Device = row.Field<string>("Device"),
                            IsAllowed = row.Field<bool>("IsAllowed"),
                            LastUpdateDate = row.Field<DateTime>("LastUpdateDate"),
                            Token = row.Field<string>("Token"),
                        }).ToList();
                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objDevices));
                }
                else
                    return StatusCode((int)HttpStatusCode.Created, "No Records found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/alldevicesbydomain")]
        public async Task<IActionResult> AllDeviceAppByDomain([FromBody] UserDomain user)
        {
            try
            {
                oData.MobileAppDeviceGetAllDeviceByUsernameAndDomain(user.Username,
                    user.Domain, user.OfferingCategory, "tMobileAppDeviceGetAllDeviceByUsernameAndDomain");
                if (this.LocalDS.Tables["tMobileAppDeviceGetAllDeviceByUsernameAndDomain"] != null
                    && this.LocalDS.Tables["tMobileAppDeviceGetAllDeviceByUsernameAndDomain"].Rows.Count > 0)
                {
                    var objDevices = this.LocalDS.Tables["tMobileAppDeviceGetAllDeviceByUsernameAndDomain"].AsEnumerable()
                        .Select(row => new tblMobileAppDevice
                        {
                            id = row.Field<int>("id"),
                            Username = row.Field<string>("Username"),
                            License = row.Field<string>("License"),
                            Device = row.Field<string>("Device"),
                            IsAllowed = row.Field<bool>("IsAllowed"),
                            Domain = row.Field<string>("Domain"),
                            OfferingCategory = row.Field<string>("OfferingCategory"),
                            LastUpdateDate = row.Field<DateTime>("LastUpdateDate"),
                            Token = row.Field<string>("Token"),
                        }).ToList();
                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objDevices));
                }
                else
                    return StatusCode((int)HttpStatusCode.Created, "No Records found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/validate")]
        public async Task<IActionResult> DeviceValidate([FromBody] LicenseAuth user)
        {
            try
            {
                oData.MobileAppDeviceGetByUserLicense(user.Username, user.License, user.Device, "tMobileAppDeviceGetByUserLicense");
                if (this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"] != null
                    && this.LocalDS.Tables["tMobileAppDeviceGetByUserLicense"].Rows.Count > 0)
                {
                    var isAllowed = Convert.ToBoolean(this.LocalDS.Tables["MobileAppDeviceGetByUserLicense"].Rows[0]["IsAllowed"].ToString().Trim());
                    return StatusCode((int)HttpStatusCode.OK, isAllowed);
                }
                else
                    return StatusCode((int)HttpStatusCode.Created, "No Records found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/updatetoken")]
        public async Task<IActionResult> UpdateToken([FromBody] UserDomain user)
        {
            try
            {
                if (user == null)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid Parameters");
                }

                var res = oData.MobileAppDeviceCheckExists(user.Username, user.Device, user.Domain, user.OfferingCategory);
                if (!res)
                {
                    return StatusCode((int)HttpStatusCode.Created, "Not Found");
                }
                else
                {
                    res = oData.MobileAppDeviceUpdate(user.Username, user.Device, user.Domain, user.OfferingCategory, user.Token);
                    if (res)
                        return StatusCode((int)HttpStatusCode.OK, "Success");
                    else
                        return StatusCode((int)HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("app/CheckUserExist")]
        public IActionResult CheckUserExist(string userName)
        {
            try
            {
                var res =  oData.UserSelectByEmail(userName);
                if (res.Item1)
                {
                    return StatusCode((int)HttpStatusCode.OK, "User Exist");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Created, "User Not found");
                }

            }
            catch(Exception e)
            {
                return StatusCode((int)HttpStatusCode.NonAuthoritativeInformation, e.Message);
            }
        }

        #endregion

        #region KN
        [HttpPost("nucleus/authenticate")]
        public async Task<IActionResult> AuthenticateKNucleus([FromBody] UserAuth user)
        {
            NucleusResponse nucleusResponse = new NucleusResponse();
            nucleusResponse.UserValid = false;
            nucleusResponse.LicenseValid = false;
            try
            {
                oData.AuthenticateLogin(user.Username, Secure.Encryptdata(user.Password), "tAuthenticateLogin");
                if (this.LocalDS.Tables["tAuthenticateLogin"] != null && this.LocalDS.Tables["tAuthenticateLogin"].Rows.Count > 0)
                {
                    nucleusResponse.UserValid = true;
                    //checking for license
                    var nUserId = Convert.ToInt32(this.LocalDS.Tables["tAuthenticateLogin"].Rows[0]["Id"].ToString());
                    var licenseKey = oData.GetLicenseKeyByUserId(nUserId, user.OfferingCategory);
                    if (string.IsNullOrEmpty(licenseKey))
                    {
                        nucleusResponse.msg = "No License assigned, Please contact to administrator";
                        return StatusCode((int)HttpStatusCode.InternalServerError, nucleusResponse.msg);
                    }
                    else
                    {
                        oData.AuthenticateLicense(user.Username, licenseKey, "tAuthenticate");
                        if (this.LocalDS.Tables["tAuthenticate"] != null && this.LocalDS.Tables["tAuthenticate"].Rows.Count > 0)
                        {
                            var expiryDate = Convert.ToDateTime(this.LocalDS.Tables["tAuthenticate"].Rows[0]["ExpiryDate"].ToString());
                            if (expiryDate < DateTime.Now)
                            {
                                return StatusCode((int)HttpStatusCode.InternalServerError, "License Expired, Please contact to administrator");
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
                                var _companySchema = this.LocalDS.Tables["tAuthenticate"].Rows[0]["CompanySchema"].ToString().Trim();
                                var _licenesekey = licenseKey;

                                double nDaysLeft = 0;
                                if (_expiryDate != null && DateTime.TryParse(_expiryDate, out DateTime dtExpiryDate))
                                {
                                    var currentDate = DateTime.Now.Date;
                                    nDaysLeft = (dtExpiryDate - currentDate).TotalDays;
                                }

                                //
                                //check for device id
                                var ExisitingDeviceId = this.LocalDS.Tables["tAuthenticate"].Rows[0]["DeviceId"].ToString().Trim();
                                if (string.IsNullOrEmpty(ExisitingDeviceId))
                                {
                                    oData.UpdateLicenseDeviceId(licenseKey, user.Device);
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
                                        role = _role,
                                        licenseley = _licenesekey,
                                        companySchema = _companySchema,
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
                                            role = _role,
                                            licenseley = _licenesekey,
                                            companySchema = _companySchema,
                                        };
                                        return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(obj).ToString().Trim());
                                    }
                                }
                            }
                        }
                        else
                        {
                            nucleusResponse.msg = "Invalid License";
                            return StatusCode((int)HttpStatusCode.InternalServerError, nucleusResponse.msg);
                        }
                    }
                }
                else
                {
                    nucleusResponse.msg = "Invalid username and password";
                    return StatusCode((int)HttpStatusCode.InternalServerError, nucleusResponse.msg);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("nucleus/changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserAuth user)
        {
            NucleusChngPwdRspn chngPwdRspn = new NucleusChngPwdRspn();
            try
            {
                oData.NucleusCheckUser(user.Username, user.Device, "tCheckUser");
                if (this.LocalDS.Tables["tCheckUser"] != null && this.LocalDS.Tables["tCheckUser"].Rows.Count > 0)
                {
                    string mailBody = (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html")))
                           ? System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "Resource", "Mail", "mailbody.html"))
                           : string.Empty;

                    string sMailOutput = "";
                    string sMailSubject = "Kockpit Nucleus Credentials";
                    string Password = Secure.Encryptdata(GeneratePassword(8));
                    string sMailBody = string.Format(mailBody.ToString(), user.Username, Secure.Decryptdata(Password));

                    List<string> ccemails = null;
                    var success = oData.NucleusUpdatePassword(user.Username, user.Device, Password);
                    if (success)
                    {

                        if (!SendMail(user.Username, sMailSubject, sMailBody, ccemails, out sMailOutput))
                        {
                            chngPwdRspn.message = "Mail Not Sent";
                            chngPwdRspn.status = false;
                            return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(chngPwdRspn).ToString().Trim());                            
                        }
                        else
                        {
                            chngPwdRspn.message = $"New Password Sent to emailid:{user.Username}";
                            chngPwdRspn.status = true;
                            return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(chngPwdRspn).ToString().Trim());
                        }
                    }
                    else
                    {
                        chngPwdRspn.message = "Password Not Updated";
                        chngPwdRspn.status = false;
                        return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(chngPwdRspn).ToString().Trim());
                    }
                }
                else
                {
                    chngPwdRspn.message = "Invalid user";
                    chngPwdRspn.status = false;
                    return StatusCode((int)HttpStatusCode.Created, JsonConvert.SerializeObject(chngPwdRspn).ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("nucleus/devices")]
        public async Task<IActionResult> GetKNDevice(int userid)
        {
            try
            {
                oData.NucleusDeviceGetByUser(userid, "tKNDeviceGetAllDevice");
                if (this.LocalDS.Tables["tKNDeviceGetAllDevice"] != null
                    && this.LocalDS.Tables["tKNDeviceGetAllDevice"].Rows.Count > 0)
                {
                    var objDevices = this.LocalDS.Tables["tKNDeviceGetAllDevice"].AsEnumerable()
                        .Select(a => new 
                        {
                            id = a.Field<int>("id"),
                            emailid = a.Field<string>("emailid")!=null?a.Field<string>("emailid").ToString():"",
                            deviceid = a.Field<string>("deviceid") != null ? a.Field<string>("deviceid").ToString() : "",
                            activationdate = a.Field<DateTime>("activationdate"),
                            licensekey=a.Field<string>("licensekey")!=null? a.Field<string>("licensekey"):""
                        }).ToList();
                    return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(objDevices));
                }
                else
                    return StatusCode((int)HttpStatusCode.Created, "No Records found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        private bool SendMail(string username, string sMailSubject, string sMailBody, List<string> ccemails, out string sMailOutput)
        {
            try
            {
                string strSenderEmail = _baseConfig["MailCredential:Email"] != null ? _baseConfig["MailCredential:Email"].ToString() : "";
                string strSenderPassword = _baseConfig["MailCredential:Password"] != null ? _baseConfig["MailCredential:Password"].ToString() : "";
                int strSenderPort = _baseConfig["MailCredential:Port"] != null ? Convert.ToInt32(_baseConfig["MailCredential:Port"].ToString()) : 587;
                string strSenderHost = _baseConfig["MailCredential:Host"] != null ? _baseConfig["MailCredential:Host"].ToString() : "";
                bool lEnableSsl = _baseConfig["MailCredential:EnableSsl"] != null ? Convert.ToBoolean(_baseConfig["MailCredential:EnableSsl"].ToString()) : true;

                Mail mail = new Mail(strSenderEmail, strSenderPassword, strSenderPort, strSenderHost, lEnableSsl);
                mail.SendMail(username, sMailSubject, sMailBody, ccemails, out sMailOutput);
                return true;
            }
            catch (Exception ex)
            {
                sMailOutput = ex.Message;
                return false;
            }
        }

        public string GeneratePassword(int passLength)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz@#$&ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, passLength)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
        #endregion
    }

    public class AcessIdAuth
    {
        public string EmpCode { get; set; }
        public string Domain { get; set; }
    }


    public class ActiveDevice
    {
        public int UserId { get; set; }
        public string LicenseKey { get; set; }
    }

    public class Response
    {
        public string message { get; set; }
        public bool status { get; set; }
    }

    public class UserAuth
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string OfferingCategory { get; set; }
        public string Device { get; set; }
        public string Domain { get; set; }
        public string Token { get; set; }
    }
    public class UserDomain
    {
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Device { get; set; }
        public string OfferingCategory { get; set; }
        public string Token { get; set; }
    }

    public class UserList
    {
        public int id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string EmpCode { get; set; }
        public string CompanyDomain { get; set; }
    }

    public class MobileAppResponse
    {
        public bool UserValid { get; set; }
        public bool LicenseValid { get; set; }
        public int Companyid { get; set; }
        public string Company { get; set; }
        public string Domain { get; set; }
        public string Logo { get; set; }
        public string Socketid { get; set; }
        public string UserId { get; set; }
        public string PhoneNo { get; set; }
        public string whatsappno { get; set; }
        public string msg { get; set; }
    }

    public class NucleusResponse
    {
        public bool UserValid { get; set; }
        public bool LicenseValid { get; set; }
        public int Companyid { get; set; }
        public string Company { get; set; }
        public string Domain { get; set; }
        public string Logo { get; set; }
        public string Socketid { get; set; }
        public string UserId { get; set; }
        public string PhoneNo { get; set; }
        public string whatsappno { get; set; }
        public string msg { get; set; }
    }

    public class NucleusChngPwdRspn
    {
        public string message { get; set; }
        public bool status { get; set; }
    }
}

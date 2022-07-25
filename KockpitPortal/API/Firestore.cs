using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using KockpitPortal.Utility;
using KockpitPortal.Models.Firebase;
using KockpitPortal.DataAccessLayer;
using System.Data;
using KockpitUtility.Common;

namespace KockpitPortal.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Firestore : ControllerBase
    {
        private readonly FirestoreProvider _firestoreProvider;
        private IConfiguration _config;
        System.Threading.CancellationToken cancellationToken;
        DataPostgres oDataAuthenticator;
        DataSet LocalDS = new DataSet();

        public Firestore(IConfiguration config)
        {
            cancellationToken = new System.Threading.CancellationToken();
            _config = config;
            oDataAuthenticator = new DataPostgres(LocalDS, KockpitAuthenticatorConnection());

            var objFireBase = new Firebase
            {
                type = _config["Firebase:type"].ToString().Trim(),
                project_id = _config["Firebase:project_id"].ToString().Trim(),
                private_key_id = _config["Firebase:private_key_id"].ToString().Trim(),
                private_key = _config["Firebase:private_key"].ToString().Trim(),
                client_email = _config["Firebase:client_email"].ToString().Trim(),
                client_id = _config["Firebase:client_id"].ToString().Trim(),
                auth_uri = _config["Firebase:auth_uri"].ToString().Trim(),
                token_uri = _config["Firebase:token_uri"].ToString().Trim(),
                auth_provider_x509_cert_url = _config["Firebase:auth_provider_x509_cert_url"].ToString().Trim(),
                client_x509_cert_url = _config["Firebase:client_x509_cert_url"].ToString().Trim(),
            };

            _firestoreProvider = new FirestoreProvider(
               new FirestoreDbBuilder
               {
                   ProjectId = objFireBase.project_id,
                   JsonCredentials = JsonConvert.SerializeObject(objFireBase)
               }.Build()
           );
        }

        public string KockpitAuthenticatorConnection(string strDatabase = "")
        {
            PostgreConnection postgreConnection = new PostgreConnection();
            postgreConnection.Host = _config["KockpitAuthenticatorPsql:Host"] != null ? _config["KockpitAuthenticatorPsql:Host"].ToString() : "";
            postgreConnection.Port = _config["KockpitAuthenticatorPsql:Port"] != null ? _config["KockpitAuthenticatorPsql:Port"].ToString() : "";
            postgreConnection.Username = _config["KockpitAuthenticatorPsql:Username"] != null ? _config["KockpitAuthenticatorPsql:Username"].ToString() : "";
            postgreConnection.Password = _config["KockpitAuthenticatorPsql:Password"] != null ? _config["KockpitAuthenticatorPsql:Password"].ToString() : "";
            postgreConnection.Database = string.IsNullOrEmpty(strDatabase)
                ? _config["KockpitAuthenticatorPsql:Database"] != null ? _config["KockpitAuthenticatorPsql:Database"].ToString() : ""
                : strDatabase;
            return string.IsNullOrEmpty(postgreConnection.Port)
                ? $"Host={postgreConnection.Host};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}"
                : $"Host={postgreConnection.Host};Port={postgreConnection.Port};Username={postgreConnection.Username};Password={postgreConnection.Password};Database={postgreConnection.Database}";
        }


        [HttpPost("getusers")]
        public async Task<IActionResult> GetUsers(string company)
        {
            try
            {
                var res = await _firestoreProvider.GetAll<Users>($"Organisations/{company}/users", cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("checkuser")]
        public async Task<IActionResult> CheckUser(string company, string email)
        {
            try
            {
                var res = await _firestoreProvider.CheckUserByEmail<Users>($"Organisations/{company}/users",
                    $"eMailId", email, cancellationToken);
                if(res != null && res.Count() > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, true);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, false);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("checkuseractive")]
        public async Task<IActionResult> CheckUserActive(string company, string email)
        {
            try
            {
                var res = await _firestoreProvider.CheckUserByEmail<Users>($"Organisations/{company}/users",
                    $"eMailId", email, cancellationToken);
                if (res != null && res.Count() > 0)
                {
                    var isActive = res.FirstOrDefault().IsActive;
                    return StatusCode((int)HttpStatusCode.OK, isActive);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "User not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string company, [FromBody] Users user)
        {
            try
            {
                await _firestoreProvider.Add($"Organisations/{company}/users", user, cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, "Created");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(string company, [FromBody] Users user)
        {
            try
            {
                await _firestoreProvider.Update($"Organisations/{company}/users",
                    _firestoreProvider.GetKey($"Organisations/{company}/users", user.eMailId, cancellationToken).Result,
                    user, 
                    cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, "Updated");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove(string company, [FromBody] Users user)
        {
            try
            {
                await _firestoreProvider.Remove($"Organisations/{company}/users", 
                    _firestoreProvider.GetKey($"Organisations/{company}/users", user.eMailId, cancellationToken).Result,
                    cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, "Removed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }


        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        #region [Sameer]
        [HttpPost("getusernames")]
        public async Task<IActionResult> GetUsername(string company)
        {
            try
            {
                var res = await _firestoreProvider.GetAll<Users>($"Organisations/{company}/users", cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, res.Select(c => c.UserName).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("getuserkey")]
        public async Task<IActionResult> GetUserKey(string company, string Username)
        {
            try
            {
                var res = await _firestoreProvider.GetKey($"Organisations/{company}/users", Username, cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("sendimage")]
        public async Task<IActionResult> SendImage(string company, [FromBody] chat chat)
        {
            try
            {
                var uid1 = chat.sender_id;
                var receiverName = chat.receiver_id;
                var receiverId = await _firestoreProvider.GetKeyByName($"Organisations/{company}/users", receiverName, cancellationToken);
                var uid2 = receiverId;

                var c = string.Compare(uid1, uid2, StringComparison.InvariantCultureIgnoreCase);
                var chatId = string.Compare(uid1,uid2, StringComparison.InvariantCultureIgnoreCase) == 1 ?  string.Concat(uid1,"-",uid2) : string.Concat(uid2, "-", uid1);
                var objMessage = new Message
                {
                    msg = chat.caption,
                    image = chat.imgUrl,
                    sender = chat.sender_id,
                    Receiver = uid2,
                    userguestname = receiverName,
                    createdAt = GetTimestamp(DateTime.Now)
                };

                var chatMode = chat.is_group ? "GroupChatFbase" : "OneToOneChat";

                await _firestoreProvider.AddMessage<Message>($"Organisations/{company}/users/{uid1}/{chatMode}/{chatId}/messages", objMessage, cancellationToken);
                return StatusCode((int)HttpStatusCode.OK, "");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }
        #endregion

        #region[Abhishek]
        [HttpPost("updatefirebaseuser")]
        public async Task<IActionResult> UpdateFirebaseUser(string company)
        {
            try
            {
                var res = await _firestoreProvider.GetAll<Users>($"Organisations/{company}/users", cancellationToken);
                var user = res.Where(s => s.eMailId == "aman.kumar@kockpit.in").FirstOrDefault();
                oDataAuthenticator.GetUsersEmpCodeByEmail(user.eMailId, "tGetUserInfo");
                if (this.LocalDS.Tables["tGetUserInfo"] != null && this.LocalDS.Tables["tGetUserInfo"].Rows.Count > 0)
                {
                    var empCode = this.LocalDS.Tables["tGetUserInfo"].Rows[0]["empcode"].ToString();

                    var objFireBaseUser = new KockpitPortal.Models.Firebase.Users
                    {
                        CompanyDomain = company,
                        userId = empCode,
                        UserName = user.UserName,
                        eMailId = user.eMailId,
                        IsActive = user.IsActive,
                    };

                    await _firestoreProvider.Update($"Organisations/{company}/users",
                 _firestoreProvider.GetKey($"Organisations/{company}/users", objFireBaseUser.eMailId, cancellationToken).Result,
                 objFireBaseUser,
                 cancellationToken);

                }

                return StatusCode((int)HttpStatusCode.OK, "succesfully update the users");
            }
            catch(Exception e)
            {
                return StatusCode(500, $"Error : {e.Message}");
            }
        }
        #endregion
    }

    public class Firebase
    {
        public string type { get; set; }
        public string project_id { get; set; }
        public string private_key_id { get; set; }
        public string private_key { get; set; }
        public string client_email { get; set; }
        public string client_id { get; set; }
        public string auth_uri { get; set; }
        public string token_uri { get; set; }
        public string auth_provider_x509_cert_url { get; set; }
        public string client_x509_cert_url { get; set; }
    }

}

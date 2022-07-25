using KockpitPortal.Utility;
using KockpitUtility.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KockpitPortal.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMTP : ControllerBase
    {
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SmtpQuery mail, bool direct = false)
        {
            try
            {
                string outMailMsg = string.Empty;
                bool res = false;
                if (direct)
                {
                    var queryToExecute = JsonConvert.SerializeObject(mail);
                    SocketManager socketManager = new SocketManager(mail.Socketid);
                    var socketResponse = socketManager.SocketResponse(eQueryType.SendMail, queryToExecute);
                    if (socketResponse.success)
                    {
                        return StatusCode((int)HttpStatusCode.OK, "Mail Sent");
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, socketResponse.msg);
                    }
                }
                else
                {
                    Mail sendMail = new Mail(mail.SenderMail, mail.SenderPassword);
                    res = sendMail.SendMail(mail.Receivers, mail.Subject, mail.Body, out outMailMsg);
                }

                if (res)
                    return StatusCode((int)HttpStatusCode.OK, "Mail Sent");
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError, outMailMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }

        [HttpPost("sendsmtp")]
        public async Task<IActionResult> SendSmtp([FromBody] SmtpQuery mail)
        {
            try
            {
                string outMailMsg = string.Empty;
                bool res = false;

                if (!string.IsNullOrEmpty(mail.Socketid))
                {
                    var queryToExecute = JsonConvert.SerializeObject(mail);
                    SocketManager socketManager = new SocketManager(mail.Socketid);
                    var socketResponse = socketManager.SocketResponse(eQueryType.SendMail, queryToExecute);
                    if (socketResponse.success)
                        return StatusCode((int)HttpStatusCode.OK, "Mail Sent");
                    else
                        return StatusCode((int)HttpStatusCode.InternalServerError, socketResponse.msg);
                }
                else
                {
                    Mail sendMail = new Mail(mail.SmtpServer.Email, mail.SmtpServer.Password, mail.SmtpServer.Port, mail.SmtpServer.Host);
                    res = sendMail.SendMail(mail.Receivers, mail.Subject, mail.Body, out outMailMsg);
                }

                if (res)
                    return StatusCode((int)HttpStatusCode.OK, "Mail Sent");
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError, outMailMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error : {ex.Message}");
            }
        }
    }
}

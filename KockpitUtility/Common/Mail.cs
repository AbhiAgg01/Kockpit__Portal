using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace KockpitUtility.Common
{
    public class Mail
    {
        string _SenderEmail = string.Empty;
        string _SenderPassword = string.Empty;
        int _SenderPort = 587;
        string _SenderHost = string.Empty;
        bool _EnableSsl = true;

        public Mail(string SenderEmail, string SenderPassword,
            int SenderPort = 587, string SenderHost = "smtp.gmail.com", bool EnableSsl = true)
        {
            _SenderEmail = SenderEmail;
            _SenderPassword = SenderPassword;
            _SenderPort = SenderPort;
            _SenderHost = SenderHost;
            _EnableSsl = EnableSsl;
        }

        public bool SendMail(string strReceiver, string strSubject, string strBody, List<string> ccEmails, out string sentMsg)
        {
            try
            {
                if (!string.IsNullOrEmpty(_SenderEmail)
                    && !string.IsNullOrEmpty(_SenderPassword)
                    && !string.IsNullOrEmpty(_SenderHost))
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtp = new SmtpClient();
                    message.From = new MailAddress(_SenderEmail);
                    message.To.Add(new MailAddress(strReceiver));
                    message.Subject = strSubject;
                    message.IsBodyHtml = true;
                    message.Body = strBody;

                    if (ccEmails != null && ccEmails.Count > 0)
                    {
                        foreach (var item in ccEmails)
                        {
                            message.CC.Add(new MailAddress(item.Trim()));
                        }
                    }

                    smtp.Port = _SenderPort;
                    smtp.Host = _SenderHost;
                    smtp.EnableSsl = _EnableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_SenderEmail, _SenderPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                    sentMsg = "Sent Success";
                    return true;
                }
                else
                {
                    sentMsg = "No mail setting found in app.config";
                    return false;
                }
            }
            catch (Exception ex)
            {
                sentMsg = ex.Message;
                return false;
            }
        }

        public bool SendMail(List<string> liReceiver, string strSubject, string strBody, out string sentMsg)
        {
            try
            {
                if (!string.IsNullOrEmpty(_SenderEmail)
                    && !string.IsNullOrEmpty(_SenderPassword)
                    && !string.IsNullOrEmpty(_SenderHost))
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtp = new SmtpClient();
                    message.From = new MailAddress(_SenderEmail);
                    foreach(var s in liReceiver)
                    {
                        message.To.Add(new MailAddress(s));
                    }
                    message.Subject = strSubject;
                    message.IsBodyHtml = true;
                    message.Body = strBody;
                    smtp.Port = _SenderPort;
                    smtp.Host = _SenderHost;
                    smtp.EnableSsl = _EnableSsl;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = new NetworkCredential(_SenderEmail, _SenderPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                    sentMsg = "Sent Success";
                    return true;
                }
                else
                {
                    sentMsg = "No mail setting found in app.config";
                    return false;
                }
            }
            catch (Exception ex)
            {
                sentMsg = ex.Message;
                return false;
            }
        }

    }
}

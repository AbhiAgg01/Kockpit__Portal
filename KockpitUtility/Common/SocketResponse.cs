using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KockpitUtility.Common
{
    public class SocketResponse
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public string jsonresponse { get; set; }
    }

    public class SmtpQuery
    {
        public string SenderMail { get; set; }
        public string SenderPassword { get; set; }
        public string Socketid { get; set; }
        public List<string> Receivers { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public SmtpServer SmtpServer { get; set; }

    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }


}

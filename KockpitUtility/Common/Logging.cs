using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace KockpitUtility.Common
{
    public class Logging
    {
        public string Action { get; set; }
        public int UserId { get; set; }
        public string EmailId { get; set; }
        public string Username { get; set; }
    }

    public class CreateLog
    {
        private string sFileTime;  // string variable for Error Time
        private Object lockLogEvent = new object();
        public string _wwwroot;

        public CreateLog(string wwwroot)
        {
            _wwwroot = wwwroot;
        }

        /// <summary>
        /// method to create a event log
        /// </summary>
        /// <param name="strDescriptions">Log Description</param>
        public void WriteLog(string strDescriptions)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(_wwwroot, @"Logs\")))
                {
                    Directory.CreateDirectory(Path.Combine(_wwwroot, @"Logs\"));

                    //DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(_wwwroot, @"Logs\"));
                    //DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    //dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    //dInfo.SetAccessControl(dSecurity);
                }

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();
                sFileTime = sDay + "_" + sMonth + "_" + sYear;
                string sDatetime = DateTime.Now.ToString();

                string sPathName = Path.Combine(_wwwroot, @"Logs\");
                if (System.IO.Directory.Exists(sPathName))
                {
                    StreamWriter sw = new StreamWriter(sPathName + sFileTime, true);
                    sw.WriteLine(sDatetime + "==>" + strDescriptions);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception) { }
        }
    }

}

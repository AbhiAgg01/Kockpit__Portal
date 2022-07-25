using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models
{
    public class Dashboard
    {
    }

    public class SuperAdminDashboard
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int LicenseRequests { get; set; }

        public int ExpiryClients { get; set; }
        public int TotalEmployees { get; set; }

        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int InActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }

        public List<tblUsers> ClientList { get; set; }
        public List<tblUsers> SupportManager { get; set; }
        public List<tblUsers> SupportRepresentative { get; set; }

    }
}

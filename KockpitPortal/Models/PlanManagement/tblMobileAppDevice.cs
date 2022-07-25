using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblMobileAppDevice
    {
        public int id { get; set; }
        public string Username { get; set; }
        public string License { get; set; }
        public string Device { get; set; }
        public bool IsAllowed { get; set; }
        public string Domain { get; set; }
        public string OfferingCategory { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Token { get; set; }
    }
}
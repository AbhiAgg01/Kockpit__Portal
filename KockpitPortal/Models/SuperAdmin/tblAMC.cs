using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblAMC
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int PlanId { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime AMCStartDate { get; set; }
        public DateTime AMCEndDate { get; set; }
        public string BudgetedManDays { get; set; }
        public bool ActiveStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsExpired { get; set; }
        public string Remarks { get; set; }
        public bool IsReissue { get; set; }
        public int IsService { get; set; }
        public int IsOfferings { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblPlanPurchaseDetail
    {
        public int Id { get; set; }
        public string SubscriptionId { get; set; }
        public int PlanId { get; set; }
        public string LicenseKey { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UserId { get; set; }
        public bool IsRenew { get; set; }
        public bool IsExpired { get; set; }

        public string TransferFrom { get; set; }

        public string DeviceId { get; set; }

        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public bool IsWarranty { get; set; }
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public bool IsTransfered { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblPlanPurchase
    {
        public int Id { get; set; }
        public string SubscriptionId { get; set; }
        public int ClientId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMode { get; set; }
        public string PaymentTransactionNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
    }
}

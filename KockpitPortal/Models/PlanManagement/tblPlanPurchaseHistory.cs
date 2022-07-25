using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblPlanPurchaseHistory
    {
        public int Id { get; set; }
        public int PlanPurchaseDetailId { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UserId { get; set; }
    }
}

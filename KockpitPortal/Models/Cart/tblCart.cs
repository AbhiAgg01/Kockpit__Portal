using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Cart
{
    public class tblCart
    {
        public int Id { get; set; }
        public int OfferingId { get; set; }
        public string OfferingName { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public int UserId { get; set; }
        public int TotalLicense { get; set; }
        public int Price { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedate { get; set; }
    }
}

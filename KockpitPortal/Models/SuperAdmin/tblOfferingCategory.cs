using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblOfferingCategory
    {
        public int Id { get; set; }
        public string OfferingCategory { get; set; }
        public string Description { get; set; }
        public bool ActiveStatus { get; set; }
        public bool IsPro { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblFAQ
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int OfferingId { get; set; }
        public bool ActiveStatus { get; set; }
        public DateTime CreatedOn{ get; set; }
    }
}

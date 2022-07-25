using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblTicketIssue
    {
        public int Id { get; set; }
        public string Issue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
    }
}

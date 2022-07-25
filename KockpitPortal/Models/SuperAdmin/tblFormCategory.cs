using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblFormCategory
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
    }
}

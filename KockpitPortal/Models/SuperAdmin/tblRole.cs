using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblRole
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
    }
}

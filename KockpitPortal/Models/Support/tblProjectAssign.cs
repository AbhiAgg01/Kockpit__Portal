using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblProjectAssign
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int EmpId { get; set; }
        public DateTime AssignDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblTicketLevel
    {
        public int Id { get; set; }

        public string LevelName { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool ActiveStatus { get; set; }
    }
}

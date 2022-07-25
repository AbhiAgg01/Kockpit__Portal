using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblServiceViewModel
    {
        public List<tblService> services { get; set; }
        public tblService service { get; set; }
    }
}

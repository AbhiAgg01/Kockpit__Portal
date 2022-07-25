using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblModuleViewModel
    {
        public List<tblModule> modules { get; set; }
        public tblModule module { get; set; }
    }
}

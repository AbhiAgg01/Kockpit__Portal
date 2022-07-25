using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblProjectViewModel
    {
        public List<tblProject> projects { get; set; }
        public tblProject project { get; set; }
    }
}

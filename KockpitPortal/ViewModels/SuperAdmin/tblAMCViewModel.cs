using KockpitPortal.Models;
using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblAMCViewModel
    {
        public List<MyPlans> plans { get; set; }
        public tblAMC amc { get; set; }
    }
}

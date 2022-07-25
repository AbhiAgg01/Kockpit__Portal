using KockpitPortal.Models.PlanManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.PlanManagement
{
    public class tblPlanViewModel
    {
        public List<tblPlan> plans { get; set; }
        public tblPlan plan { get; set; }
        public Dictionary<string, List<tblPlan>> data { get; set; }
    }
}

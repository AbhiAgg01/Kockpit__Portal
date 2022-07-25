using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblPlanRequestDetail
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public int ValidityDays { get; set; }
        public string PlanPrice { get; set; }
        public int NoOfUsers { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ProjectImage { get; set; }
        public bool IsWarranty { get; set; }
        public double WarrantyInDays { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.PlanManagement
{
    public class tblPlan
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int NoOfUsers { get; set; }
        public int ValidityDays { get; set; }
        public bool IsFree { get; set; }
        public string PlanPrice { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public bool IsWarranty { get; set; }
        public int? WarrantyInDays { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}

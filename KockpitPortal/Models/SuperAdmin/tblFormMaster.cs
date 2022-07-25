using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblFormMaster
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string FormName { get; set; }
        public string Description { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string PageCode { get; set; }
        public string LinkIcon { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public string ModuleName { get; set; }
        public string ModuleIds { get; set; }
    }
}

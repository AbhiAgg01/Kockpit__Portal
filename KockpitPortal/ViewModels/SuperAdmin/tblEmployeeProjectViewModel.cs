using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblEmployeeProjectViewModel
    {
        public List<tblEmployeeProject> employeeProjects { get; set; }
        public tblEmployeeProject employeeProject { get; set; }
    }
}

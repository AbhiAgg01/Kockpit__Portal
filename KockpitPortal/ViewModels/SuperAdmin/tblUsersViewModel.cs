using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblUsersViewModel
    {
        public List<tblUsers> users { get; set; }
        public tblUsers user { get; set; }
    }
}

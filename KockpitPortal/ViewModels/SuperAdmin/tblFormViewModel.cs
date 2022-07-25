using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblFormViewModel
    {
        public List<tblFormMaster> forms { get; set; }
        public tblFormMaster form { get; set; }
        public tblFormDetail formModules { get; set; }
    }
}

using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblFormCategoryViewModel
    {
        public List<tblFormCategory> formCategories { get; set; }
        public tblFormCategory formCategory { get; set; }
    }
}

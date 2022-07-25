using KockpitPortal.Models.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.SuperAdmin
{
    public class tblOfferingCategoryViewModel
    {
        public List<tblOfferingCategory> offeringCategories { get; set; }
        public tblOfferingCategory offeringCategory { get; set; }
    }
}

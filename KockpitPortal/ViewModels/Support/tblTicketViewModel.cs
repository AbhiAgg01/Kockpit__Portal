using KockpitPortal.Models;
using KockpitPortal.Models.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.ViewModels.Support
{
    public class tblTicketViewModel
    {
        public List<tblTicket> tickets { get; set; }
        public tblTicket ticket { get; set; }
      //  public List<tblFAQ> faqs { get; set; }
    }
}

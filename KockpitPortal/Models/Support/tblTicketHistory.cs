using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblTicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string Remarks { get; set; }
        public string Attachment { get; set; }
        public int AssignedTo { get; set; }
        public string TicketStatus { get; set; }
        public DateTime TicketStatusDate { get; set; }
        public string UserName { get; set; }
        public string AssignedUser { get; set; }
        public string Logo { get; set; }
        public string Resolution { get; set; }
        public string Action { get; set; }
        public string Discription {get;set;}
    }
}

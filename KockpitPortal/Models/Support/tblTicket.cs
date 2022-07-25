using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Support
{
    public class tblTicket
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public int UserId { get; set; }
        public int LicenseId { get; set; }
        public string subscriptionId { get; set; }
        public string Level { get; set; }
        public int LevelId { get; set; }
        public string TicketSubject { get; set; }
        //public string TicketQuery { get; set; }
        public string SupportRep { get; set; }

        public string Description { get; set; }
        public IFormFile file { get; set; }
        public string Attachment { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime? AssignDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string TicketStatus { get; set; }
        public string CusRating { get; set; }
        //public string CusComment { get; set; }
        public string EmpRating { get; set; }
        //public string EmpComment { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public string Scenario { get; set; }   
        public string UserName { get; set; }
        public int IssueId { get; set; }    
        public string Resolution { get; set; }
    }
}

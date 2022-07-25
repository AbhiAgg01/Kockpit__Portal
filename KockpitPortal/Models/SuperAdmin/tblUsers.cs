using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblUsers
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string SchemaName { get; set; }
        public DateTime? ExpiredOn { get; set; }
        public IFormFile file { get; set; }
        public string Logo { get; set; }
        public string Role { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string SocketId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public bool ClientConnected { get; set; }
        public string CCMailIds { get; set; }
        public bool IsSupportManager { get; set; }
        public string EmpCode { get; set; }
        public string ContactNo1 { get; set; }
        public string Contact2 { get; set; }

        public string DeviceId { get; set; }

        public string ConfirmPassword { get; set; }

        public string Address { get; set; }

    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.SuperAdmin
{
    public class tblProject
    {
        public int Id { get; set; }
        public string OfferingCategory { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string ProjectType { get; set; }
        public string ProjectImage { get; set; }
        public string ProjectVideo { get; set; }
        public string ProjectIcon { get; set; }
        public string ProjectStartUpLink { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool ActiveStatus { get; set; }
        public IFormFile file { get; set; }
        public IFormFile Videofile { get; set; }
        public IFormFile IconFile { get; set; }
        public string LicenseKey { get; set; }
        public bool IsPro { get; set; }
        public bool IsChat { get; set; }
    }
}

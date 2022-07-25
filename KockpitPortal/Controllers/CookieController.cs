using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KockpitPortal.Controllers
{
    public class CookieController : Controller
    {
        CookieOptions option;
        public CookieController()
        {
            option = new CookieOptions();
            option.Expires = DateTime.Now.AddMinutes(30);
            option.Domain = ".kockpit.in";
            //option.Domain = "localhost";
        }

        public CookieOptions CookieOpt { get { return option; } set { value = option; } }
    }
}

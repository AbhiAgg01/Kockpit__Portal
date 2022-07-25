using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitAuthenticator
{
    public class SessionData
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string License { get; set; }
        public string Company { get; set; }
        public string Domain { get; set; }
        public bool IsPro { get; set; }
    }

    public class JwtExtra
    {
        public string kid { get; set; }
        public string kpro { get; set; }
    }
}

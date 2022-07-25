using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Firebase
{
    public class chat
    {
        public string caption { get; set; }
        public string imgUrl { get; set; }
        public string sender_id { get; set; }
        public string receiver_id { get; set; }
        public bool is_group { get; set; }
    }

    public class Message
    {
        //public string text { get; set; }
        //public string media { get; set; }
        //public string from { get; set; }
        //public string to { get; set; }
        //public string senderUsername { get; set; }
        //public string createdAt { get; set; }

        public string Receiver { get; set; }
        public string createdAt { get; set; }
        public string image { get; set; }
        public img img { get; set; }
        public string messsageId { get; set; }
        public string msg { get; set; }
        public string sender { get; set; }
        public List<string> usercurrentname { get; set; }
        public string userguestname { get; set; }
    }

    public class img
    {
        public string message { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Notification
{
    public class tblNotification
    {
		public int Id { get; set; }
		public string Subject { get; set; } 
		public string Message { get; set; }
		public string Type { get; set; } 
		public bool IsRead { get; set; }
		public DateTime Createdon { get; set; }
		public int RefrenceId { get; set; }
	}
}

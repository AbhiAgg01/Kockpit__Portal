using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KockpitUtility.Common
{
    public class SuccessResponse
    {
        public string msg { get; set; }
        public bool success { get; set; }
    }

    public class RecievingResult
    {
        public string RequestId { get; set; }
        public int ErrorCode { get; set; }
        public string Result { get; set; }
    }

    public class SendingQuery
    {
        public string RequestId { get; set; }
        public string Mode { get; set; }
        public string Query { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Utility
{
    interface IQueryResult { }
    class Sample : IQueryResult
    {
        public string response { get; set; }
    }
    public enum eQueryType
    {
        SendMail
    }
    class QueryInfo
    {
        public QueryInfo(string mode, string queryToExecute, IQueryResult queryResult)
        {
            Mode = mode;
            QueryToExecute = queryToExecute;
            QueryResult = queryResult;
        }
        public string Mode { get; set; }
        public string QueryToExecute { get; set; }
        public IQueryResult QueryResult { get; set; }
    }

    static class QueryManager
    {
        public static Dictionary<eQueryType, QueryInfo> Queries = new Dictionary<eQueryType, QueryInfo>();
        static QueryManager()
        {
            Queries.Add(eQueryType.SendMail, new QueryInfo("SENDMAIL", "", new Sample()));
        }
    }
}

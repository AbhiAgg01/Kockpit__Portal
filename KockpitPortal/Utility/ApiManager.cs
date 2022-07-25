using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KockpitPortal.Utility
{
    public class ApiManager
    {
        private string _api;
        public ApiManager(string api)
        {
            _api = api;
        }

        public async Task<Tuple<HttpStatusCode, string>> Post(string jsonBody)
        {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                using (var response = await client.PostAsync(_api, content).ConfigureAwait(false))
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return new Tuple<HttpStatusCode, string>(response.StatusCode, responseContent);
                }
            }
        }

        public async Task<Tuple<HttpStatusCode, string>> Post(Dictionary<string, string> parameters)
        {
            var encodedContent = new FormUrlEncodedContent(parameters);
            using (var client = new HttpClient())
            {
                using (var response = await client.PostAsync(_api, encodedContent).ConfigureAwait(false))
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return new Tuple<HttpStatusCode, string>(response.StatusCode, responseContent);
                }
            }
        }
    }
}

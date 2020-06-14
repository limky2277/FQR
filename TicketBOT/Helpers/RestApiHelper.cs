using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TicketBOT.Helpers
{
    public class RestApiHelper
    {
        private static HttpClient _client;

        static RestApiHelper()
        {
            _client = new HttpClient();
        }

        public static Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return _client.GetAsync(requestUri);
        }

        public static Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return _client.PostAsync(requestUri, content);
        }
    }
}

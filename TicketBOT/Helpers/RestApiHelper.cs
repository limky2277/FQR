using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TicketBOT.Helpers
{
    public class RestApiHelper
    {
        private static HttpClient _client;

        static RestApiHelper()
        {            
            _client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _client.SendAsync(request);
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

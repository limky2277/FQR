using log4net.Appender;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TicketBOT.Core.Helpers
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

    public class LoggingHandler : DelegatingHandler
    {
        public static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (request.Content != null)
            {
                logger.Debug($"REQUEST Content {await request.Content.ReadAsStringAsync()}");
            }

            if (response.Content != null)
            {
                logger.Debug($"RESPONSE Content {await response.Content.ReadAsStringAsync()}");
            }

            var fa = logger.Logger.Repository.GetAppenders().OfType<RollingFileAppender>().FirstOrDefault();


            return response;
        }
    }
}

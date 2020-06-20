using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TicketBOT.Helpers
{
    public class LoggingHelper
    {
        public static void LogError(Exception ex, ILog logger, HttpRequest request, RouteData routeData)
        {
            logger.Error($"Method: {request.Method} --> {routeData.Values["action"]}", ex);
        }

        public static void LogError(Exception ex, ILog logger)
        {
            logger.Error(ex);
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

using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

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
}

using Microsoft.AspNetCore.Builder;

namespace TicketBOT.Middleware
{
    // Factory-based middleware
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/extensibility?view=aspnetcore-2.2
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseFactoryBasedLoggingMiddleware(
            this IApplicationBuilder builder)
        {
            // Api request & response logging middleware
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}

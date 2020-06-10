
namespace TicketBOT.Models
{
    public class ApplicationSettings
    {
        public FacebookApp FacebookApp { get; set; }
        public TicketBOTDb TicketBOTDb { get; set; }
        public FacebookGraphApiEndpoint FacebookGraphApiEndpoint { get; set; }
        public RedisSettings RedisSettings { get; set; }
    }

    public class FacebookApp
    {
        public string AppSecret { get; set; }
        public string CallbackVefifyToken { get; set; }
    }

    public class TicketBOTDb
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    
    public class FacebookGraphApiEndpoint
    {
        public string PostMessage { get; set; }
        public string GetProfile { get; set; }
    }
    public class RedisSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string CachingProvider { get; set; }

        public int TimeSpanMins { get; set; }

    }
}

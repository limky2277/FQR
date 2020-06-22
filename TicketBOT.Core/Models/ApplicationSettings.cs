
namespace TicketBOT.Core.Models
{
    public class ApplicationSettings
    {
        public FacebookApp FacebookApp { get; set; }
        public TicketBOTDb TicketBOTDb { get; set; }
        public FacebookGraphApiEndpoint FacebookGraphApiEndpoint { get; set; }
        public ConversationSettings ConversationSettings { get; set; }
        public SwaggerSettings SwaggerSettings { get; set; }
        public JIRAApiEndpoint JIRAApiEndpoint { get; set; }
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

    public class ConversationSettings
    {
        public int ExpiryAfterMins { get; set; }
        public int TimeToLiveMins { get; set; }

    }
    public class SwaggerSettings
    {
        public string JsonRoute { get; set; }
        public string Description { get; set; }
        public string UIEndpoint { get; set; }
    }

    public class JIRAApiEndpoint
    {
        public string GetStatus { get; set; }
        public string CreateCase { get; set; }
        public string GetServiceDesk { get; set; }
    }
}

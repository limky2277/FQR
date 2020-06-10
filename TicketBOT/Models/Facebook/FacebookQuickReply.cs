using Newtonsoft.Json;

namespace TicketBOT.Models.Facebook
{
    public class FacebookQuickReply
    {
        public const string RAISE_TICKET = "Raise a Ticket";
        public const string TICKET_STATUS = "Check Ticket Status";
        public const string JUST_BROWSE = "Just browsing";

        public class QuickReplyOption
        {
            public string content_type { get; set; } = "text";
            public string title { get; set; }
            public string payload { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string image_url { get; set; } = null;
        }
    }
}

﻿using Newtonsoft.Json;

namespace TicketBOT.Models.Facebook
{
    public class FacebookQuickReply
    {
        public const string RAISE_TICKET = "Raise a Ticket";
        public const string TICKET_STATUS = "Check Ticket Status";
        public const string JUST_BROWSE = "Just browsing";
        public const string RETRY_YES = "Yes please!";
        public const string RETRY_NO = "No, thank you.";

        public class QuickReplyOption
        {
            public string content_type { get; set; } = "text";
            public string title { get; set; }
            public string payload { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string image_url { get; set; } = null;
        }

        public class QuickReply
        {
            public string payload { get; set; }
        }

        public static string CheckQuickReplyPayload(Messaging incomingMessage)
        {
            string payload = null;
            try
            {
                payload = incomingMessage.message.quick_reply.payload;
            }
            catch { }

            return payload;
        }
    }
}

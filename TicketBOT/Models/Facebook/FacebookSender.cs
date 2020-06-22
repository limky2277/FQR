using System.Text.Json.Serialization;

namespace TicketBOT.Models.Facebook
{
    public class FacebookSender
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        [JsonIgnore]
        public string senderConversationId { get; set; }
    }
}

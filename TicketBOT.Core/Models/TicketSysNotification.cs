using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public class TicketSysNotification
    {
        public TicketSysNotification()
        {
            Id = Guid.NewGuid().ToString();
        }
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string TicketSysUserId { get; set; }
        public string OneTimeNotifToken { get; set; }
        public string JiraCaseKey { get; set; }
        public string JiraCaseStatus { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
    }
}

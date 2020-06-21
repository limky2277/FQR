using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Models
{
    public class TicketSysNotification
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TicketSysUserId { get; set; }
        public string OneTimeNotifToken { get; set; }
        public string JiraCaseKey { get; set; }
        public string JiraCaseStatus { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Models
{
    public class Company
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CompanyName { get; set; }
        public string FbPageId { get; set; }
        public string FbPageToken { get; set; }
        public string TicketSysUrl { get; set; }
        public string TicketSysId { get; set; }
        public string TicketSysPassword { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string contactEmail { get; set; }
        public bool Active { get; set; } = true;
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public class ClientCompany
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ClientCompanyName { get; set; }
        public string TicketSysCompanyCode { get; set; }
        public string VerificationEmail { get; set; }
        public string VerificationCode { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
    }
}

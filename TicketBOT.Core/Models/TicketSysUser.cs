using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public class TicketSysUser
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserFbId { get; set; }
        public Guid ClientCompanyId { get; set; }
        public Guid CompanyId { get; set; }
        public string UserNickname { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string VerificationCode { get; set; }
        public bool Active { get; set; } = true;
    }
}

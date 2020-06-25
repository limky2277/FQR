using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public class TicketSysUser
    {
        public TicketSysUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string UserFbId { get; set; }
        public string ClientCompanyId { get; set; }
        public string CompanyId { get; set; }
        public string UserNickname { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string VerificationCode { get; set; }
        public bool Active { get; set; } = true;
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using TicketBOT.Core.Helpers;

namespace TicketBOT.Core.Models
{
    public class Company
    {
        public Company()
        {
            Id = Guid.NewGuid().ToString();                
        }

        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
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

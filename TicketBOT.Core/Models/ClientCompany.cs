﻿using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public class ClientCompany
    {
        public ClientCompany()
        {
            Id = Guid.NewGuid().ToString();
        }

        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ClientCompanyName { get; set; }
        public string TicketSysCompanyCode { get; set; }
        public string VerificationEmail { get; set; }        
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
    }
}

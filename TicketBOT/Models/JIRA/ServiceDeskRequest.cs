using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketBOT.Models.JIRA
{
    public class RequestFieldValues
    {
        public string summary { get; set; }
        public string description { get; set; }
        public string duedate { get; set; }

    }

    public class ServiceDeskRequest
    {
        public ServiceDeskRequest()
        {
            requestParticipants = new  List<string>();
        }

        public string serviceDeskId { get; set; }
        public string requestTypeId { get; set; }
        public RequestFieldValues requestFieldValues { get; set; }
        public List<string> requestParticipants { get; set; }
    }
}

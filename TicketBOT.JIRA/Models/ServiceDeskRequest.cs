using System.Collections.Generic;

namespace TicketBOT.JIRA.Models
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

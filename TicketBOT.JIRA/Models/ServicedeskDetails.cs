using System.Collections.Generic;

namespace TicketBOT.JIRA.Models
{


    public class ServiceDesk
    {
       
        public string id { get; set; }
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string projectKey { get; set; }
        public string email { get; set; }
        public Links _links { get; set; }
    }

    public class ServicedeskDetails
    {
        public int size { get; set; }
        public int start { get; set; }
        public int limit { get; set; }
        public bool isLastPage { get; set; }
        public Links _links { get; set; }
        public IList<ServiceDesk> values { get; set; }
    }
}

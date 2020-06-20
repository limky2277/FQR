using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketBOT.Models
{
    public class CaseDetail
    {
        public long CaseID { get; set; }
        public string CaseKey { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Status { get; set; }
        public string Subject { get; set; }

        public string Detail { get; set; }

        public Uri WebURL { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}

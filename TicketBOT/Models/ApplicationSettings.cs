using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketBOT.Models
{
    public class ApplicationSettings
    {
        public FacebookApp FacebookApp { get; set; }
        public TicketBOTDb TicketBOTDb { get; set; }
    }

    public class FacebookApp
    {
        public string AppSecret { get; set; }
        public string CallbackVefifyToken { get; set; }
    }

    public class TicketBOTDb
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}

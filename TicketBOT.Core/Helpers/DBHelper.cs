using System;
using System.Collections.Generic;
using System.Text;

namespace TicketBOT.Core.Helpers
{
    public static class DBHelper
    {
        public static string getInfo(TicketBOT.Core.Models.ApplicationSettings sett)
        {
            string mnb = sett.General.SysInfo;
            return string.Format(sett.TicketBOTDb.ConnectionString, Utility.ParseDInfo(sett.TicketBOTDb.DBPass, mnb));
        }
    }
}

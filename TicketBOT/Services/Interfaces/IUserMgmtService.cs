
using System;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IUserMgmtService
    {
        TicketSysUser GetUser(string userFbId, Guid companyId);
    }
}

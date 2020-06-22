
using System;
using TicketBOT.Core.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IUserMgmtService
    {
        TicketSysUser GetUser(string userFbId, Guid companyId);
    }
}

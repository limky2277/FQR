
using System;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IUserMgmtService
    {
        JiraUser GetUser(string userFbId, Guid companyId);
    }
}

using System;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IConversationService
    {
        QAConversation GetLastQuestion(Guid CompanyId, string FbSenderId);
    }
}

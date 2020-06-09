using System;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IConversationService
    {
        QnAConversation GetLastQuestion(Guid CompanyId, string FbSenderId);
    }
}

using System.Collections.Generic;
using TicketBOT.Core.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IConversationService
    {
        bool AnyActiveConversation(string senderPageId);
        void UpsertActiveConversation(string senderPageId, ConversationData conversations);
        ConversationData LastConversation(string senderPageId);
        void RemoveActiveConversation(string senderPageId);
        List<ConversationData> GetConversationList(string senderPageId);
    }
}

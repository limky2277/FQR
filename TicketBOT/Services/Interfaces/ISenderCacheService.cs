
using EasyCaching.Core;
using System.Collections.Generic;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface ISenderCacheService
    {
        bool AnyActiveConversation(string senderId);
        void UpsertActiveConversation(string senderId, QAConversation conversations);
        QAConversation LastConversation(string senderId);
        void RemoveActiveConversation(string senderId);
        List<QAConversation> GetConversationList(string senderId);
    }
}

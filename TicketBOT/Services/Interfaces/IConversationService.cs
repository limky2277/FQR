using System;
using System.Collections.Generic;
using TicketBOT.Core.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IConversationService
    {
        Conversation GetActiveConversation(string senderPageId, ConvLogType convLogType = ConvLogType.ChatLog);
        void UpsertActiveConversation(string senderPageId, ConversationData conversations);
        ConversationData LastConversation(string senderPageId);
        void RemoveActiveConversation(string senderPageId);
        List<ConversationData> GetConversationList(string senderPageId);
        void Create(Conversation conversation);
        void Update(Guid id, Conversation conversation);
    }
}

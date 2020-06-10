
namespace TicketBOT.Services.Interfaces
{
    public interface ISenderCacheService
    {
        bool AnyActiveConversation(string senderId);
        void UpsertActiveConversation(string senderId, string value);
        void RemoveActiveConversation(string senderId);
    }
}

using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface IFbApiClientService
    {
        Task<SenderInfo> GetUserInfoAsync(string pageToken, string senderId);
        Task PostMessageAsync(string pageToken, JObject json);
    }
}

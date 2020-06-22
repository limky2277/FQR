
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TicketBOT.Models.Facebook;

namespace TicketBOT.Services.Interfaces
{
    public interface IFbApiClientService
    {
        Task<FacebookSender> GetUserInfoAsync(string pageToken, string senderId);
        Task PostMessageAsync(string pageToken, JObject json);
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.FacebookServices
{
    public class FbApiClientService : IFbApiClientService
    {
        private readonly HttpClient _client;

        public FbApiClientService(HttpClient client)
        {
            _client = client;
        }

        public async Task<SenderInfo> GetUserInfoAsync(string pageToken, WebhookModel webhookModel)
        {
            string senderId = webhookModel.entry.First().messaging.First().sender.id;
            
            var resp = await _client.GetAsync($"https://graph.facebook.com/{senderId}?fields=first_name,last_name&access_token={pageToken}");
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<SenderInfo>(resp.Content.ReadAsStringAsync().Result);
            }

            return null;
        }

        public async Task PostMessageAsync(string pageToken, JObject json)
        {
            await _client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
        }
    }
}

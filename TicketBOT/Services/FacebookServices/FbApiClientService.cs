using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicketBOT.Models;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.FacebookServices
{
    public class FbApiClientService : IFbApiClientService
    {
        private readonly HttpClient _client;
        private readonly ApplicationSettings _appSettings;

        public FbApiClientService(HttpClient client, ApplicationSettings appSettings)
        {
            _client = client;
            _appSettings = appSettings;
        }

        public async Task<FacebookSender> GetUserInfoAsync(string pageToken, string senderId)
        {
            var resp = await _client.GetAsync(string.Format(_appSettings.FacebookGraphApiEndpoint.GetProfile, senderId, pageToken));
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<FacebookSender>(resp.Content.ReadAsStringAsync().Result);
            }

            return null;
        }

        public async Task PostMessageAsync(string pageToken, JObject json)
        {
            await _client.PostAsync(string.Format(_appSettings.FacebookGraphApiEndpoint.PostMessage, pageToken), new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
        }
    }
}

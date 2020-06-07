using FQR.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FQR.Controllers
{
    #region References
    /*
     * https://github.com/eternaltung/MessengerBot-WebAPI
     * https://glitch.com/edit/#!/daffodil-authorization?path=bot.js%3A1154%3A23
    */
    #endregion
    public class QuickReplyController : ApiController
    {
        #region Token & Secret
        string pageToken = "EAAJHOBuXMzYBAL6UFTFdUMVr9n1qVlYT5x1g1GFMTJ4TOXshS367xj1yZA80T6dZBq9J3cxWoYDZBD3er1PXuSQAfy2trkHCUcvhJxs7w5sQbKxia5fA0wjkGr04WePRiel828uBVAAtrS8AoesZCRGVI9clLQQ6BZCG8feQzHGwYgjeviPABcG1nredccgEZD";
        string pageToken2 = "EAADcf5Tn8Q0BAGHSAzsOjfVLngSfX7sog5fQWLZBYr2Alze2vYoMZBVAXZCCodsJwCwkM69VOXffIZADMc3GP8vkqXfmtkVtulvd5tYv8EMLNmCUE0QG3no3MbbwhDOj4wEfZAoIitYVQLAkTUyf0asFnDZAV8veNmoTjwjhAT8U7P69aO3ZB3WZBuvxWOmOHigZD";
        string appSecret = "a722e1d7a9a474d27cb290bad5b0ec71";
        string verifyToken = "a1234b3214!";
        #endregion

        #region Verify Token / Secret
        public HttpResponseMessage Get()
        {
            var querystrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            if (querystrings["hub.verify_token"] == verifyToken)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(querystrings["hub.challenge"], Encoding.UTF8, "text/plain")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
        #endregion

        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            var signature = Request.Headers.GetValues("X-Hub-Signature").FirstOrDefault().Replace("sha1=", "");
            var body = await Request.Content.ReadAsStringAsync();
            if (!VerifySignature(signature, body))
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var value = JsonConvert.DeserializeObject<WebhookModel>(body);
            if (value._object != "page")
                return new HttpResponseMessage(HttpStatusCode.OK);

            //read user name here.
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //var a = value.entry.First().messaging.First().sender.id;
                var a = "abc123";
                                
                HttpResponseMessage ress = await client.GetAsync($"https://graph.facebook.com/{a}?fields=first_name,last_name&access_token={pageToken}");
                //ress.Content.ReadAsStringAsync().Result;
            }

            foreach (var item in value.entry[0].messaging)
            {
                if (item.message == null && item.postback == null) { continue; }
                else
                {
                    await SendMessage(GetMessageTemplate(item.message.text, item.sender.id));
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        #region Verify Signature
        private bool VerifySignature(string signature, string body)
        {
            var hashString = new StringBuilder();
            using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(appSecret)))
            {
                var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
                foreach (var item in hash)
                    hashString.Append(item.ToString("X2"));
            }

            return hashString.ToString().ToLower() == signature.ToLower();
        }
        #endregion

        #region Send Message (With QR buttons)
        /// <summary>
        /// send message
        /// </summary>
        /// <param name="json">json</param>
        private async Task SendMessage(JObject json)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
            }
        }
        #endregion


        #region Prepare Quick Reply Buttons
        /// <summary>
        /// get text message template
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="sender">sender id</param>
        /// <returns>json</returns>
        private JObject GetMessageTemplate(string text, string sender)
        {
            // Quick reply 
            var quickReplyOption = new List<QuickReplyOption>
            {
                new QuickReplyOption { title = "Test A", payload = "Test A Payload" },
                new QuickReplyOption { title = "Test B", payload = "Test B Payload" },
                new QuickReplyOption { title = "Test C", payload = "Test C Payload" },
            };


            return JObject.FromObject(new
            {
                recipient = new { id = sender },
                message = new { text = $"Your have choosen {text}", quick_replies = quickReplyOption }
            });
        }
        #endregion

    }
}

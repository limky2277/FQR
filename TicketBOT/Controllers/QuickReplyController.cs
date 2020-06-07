using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;

namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickReplyController : ControllerBase
    {
        private readonly ApplicationSettings _appSettings;
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly CompanyService _companyService;

        public QuickReplyController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService,
            IUserRegistrationService userRegistrationService,
            IFbApiClientService fbApiClientService, CompanyService companyService)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _userRegistrationService = userRegistrationService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
        }

        #region GET --> Verify Token / Secret
        // To be called when adding Webhooks to Facebook App
        public IActionResult Get()
        {
            if (Request.Query["hub.verify_token"] == _appSettings.FacebookApp.CallbackVefifyToken)
            {
                return Ok(Request.Query["hub.challenge"].ToString());
            }
            return StatusCode(401);
        }
        #endregion

        #region POST --> Reply message to sender
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var signature = Request.Headers["X-Hub-Signature"].FirstOrDefault().Replace("sha1=", "");
            string body = await new StreamReader(Request.Body).ReadToEndAsync();

            if (!VerifySignature(signature, body))
                return BadRequest();

            var value = JsonConvert.DeserializeObject<WebhookModel>(body);
            if (value._object != "page")
                return Ok();

            foreach (var entry in value.entry)
            {
                // To-do: retrieve pageToken from database based on Page ID
                var incomingPageId = entry.id;
                var company = _companyService.Get(incomingPageId);

                // If company registered in db
                if (company != null)
                {
                    foreach (var msgItem in entry.messaging)
                    {
                        if (msgItem.message == null && msgItem.postback == null) { continue; }
                        else
                        {
                            //read user name here. Return null if user not found
                            var userInfo = await _fbApiClientService.GetUserInfoAsync(company.FbPageToken, msgItem.sender.id);

                            await _fbApiClientService.PostMessageAsync(company.FbPageToken, GetMessageTemplate(msgItem.message.text, msgItem.sender.id));
                        }
                    }
                }
                else
                {
                    // To-do: Company not registered in db
                }
            }

            return Ok();
        }
        #endregion

        #region Verify Signature
        private bool VerifySignature(string signature, string body)
        {
            var hashString = new StringBuilder();
            using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(_appSettings.FacebookApp.AppSecret)))
            {
                var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
                foreach (var item in hash)
                    hashString.Append(item.ToString("X2"));
            }

            return hashString.ToString().ToLower() == signature.ToLower();
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

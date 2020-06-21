using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TicketBOT.BotAgent;
using TicketBOT.Helpers;
using TicketBOT.Models;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;

namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickReplyController : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ApplicationSettings _appSettings;
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly Bot _bot;
        private readonly OneTimeNotification _oneTimeNotifAgent;

        public QuickReplyController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService,
            JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService, Bot bot, OneTimeNotification oneTimeNotification)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _bot = bot;
            _oneTimeNotifAgent = oneTimeNotification;
        }

        #region GET --> Verify Token / Secret
        // To be called when adding Webhooks to Facebook App
       [HttpGet]
        public IActionResult Get()
        {
            try
            {
                if (Request.Query["hub.verify_token"] == _appSettings.FacebookApp.CallbackVefifyToken)
                {
                    return Ok(Request.Query["hub.challenge"].ToString());
                }
                return StatusCode(401);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger, this.Request, this.RouteData);
                return StatusCode(500);
            }
        }
        #endregion

        #region POST --> Reply message to sender
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                var signature = Request.Headers["X-Hub-Signature"].FirstOrDefault().Replace("sha1=", "");
                string body = await new StreamReader(Request.Body).ReadToEndAsync();

                if (!FacebookChatbotHelper.VerifySignature(_appSettings, signature, body))
                    return BadRequest();

                var value = JsonConvert.DeserializeObject<FacebookMessage>(body);
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
                            if (msgItem.message == null && msgItem.postback == null) 
                            {
                                if (!FacebookChatbotHelper.VerifyIsOneTimeNotifPayload(msgItem)) { continue; }

                                await _oneTimeNotifAgent.UserOptinCaseNoification(msgItem, company);
                            }
                            else
                            {
                                // Dispatch bot agent
                                await _bot.DispatchAgent(msgItem, company);
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
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger, this.Request, this.RouteData);
                return Ok();
            }
        }
        #endregion
    }
}

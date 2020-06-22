using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TicketBOT.Services.BotServices;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.Helpers;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.BotServices;
using TicketBOT.Services.DBServices;
using TicketBOT.Services.Interfaces;

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
        private readonly TicketSysUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly BotService _botService;
        private readonly OneTimeNotificationService _oneTimeNotifService;

        public QuickReplyController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService,
            TicketSysUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService, BotService botService, OneTimeNotificationService oneTimeNotifService)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _botService = botService;
            _oneTimeNotifService = oneTimeNotifService;
        }

        #region GET --> Verify Token / Secret
        // To be called when adding Webhooks to Facebook App
       [HttpGet]
        public IActionResult Get()
        {
            try
            {
                string xyz = _appSettings.FacebookApp.CallbackVefifyToken;
                var val = TicketBOT.Core.Helpers.Utility.ParseDInfo(_appSettings.General.SysInfo, xyz);
                if (Request.Query["hub.verify_token"] == val)
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

                                await _oneTimeNotifService.UserOptinCaseNoification(msgItem, company);
                            }
                            else
                            {
                                // Dispatch bot agent
                                await _botService.DispatchAgent(msgItem, company);
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

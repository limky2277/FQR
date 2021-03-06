﻿using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.Helpers;
using TicketBOT.Services.DBServices;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Controllers
{
    #region Reference
    // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-primitive-prompts?view=azure-bot-service-4.0&tabs=csharp
    #endregion
    [Route("api/[controller]")]
    [ApiController]
    public class PlaygroundController : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ApplicationSettings _appSettings;
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly TicketSysUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientCompanyService;

        private IConversationService _conversationService;

        public PlaygroundController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService, TicketSysUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService,
            ClientCompanyService clientCompanyService,
            IConversationService conversationService)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientCompanyService = clientCompanyService;
            _conversationService = conversationService;
        }


        [HttpGet]
        [Route("Get")]
        public IActionResult Get()
        {
            try
            {
                // string tempToken = "EAADcf5Tn8Q0BAA6DdKIRm8vQ1TuZCJA93pA893nGyZAabOsyNPJl7psqiEBZBtrrV318UBjecemc2quU3OkMQH8YGV6tR12tvZBMUzWaLuDWoell68ZB5YB0cWuvq0Phh5vFyS6av3vLqZCRR69Bdjhye05Ofs5zaI3sgfasd1Ukfkeg6LPbmuvBKtiGldx7kZD";
                // Seed Data
                string pass = Utility.ParseEInfo("DevS@b3r", _appSettings.General.SysInfo);
                string tok = Utility.ParseEInfo("EAADcf5Tn8Q0BAPqlinfwZAQ6JrcAdvq5OdpxHlFZCw4anCRga5CEq0jKipw2iEgwzADIMmAftoGloGpYiwlsa2YDaeMBRM1NCApN4XSZAbEovKiESVB5TQZBHClzs13voVQZBffPRZB5gZA3ixnZAUZCa2Bm87HJzNuKvvzrt1HcdJ9WO6ygKOq28qVzQMTWTJsUZD", _appSettings.General.SysInfo);
                var companyResult = _companyService.Create(new Company { CompanyName = "Sabre", FbPageId = "102327571503111", FbPageToken = tok, TicketSysUrl = "http://58.185.112.2:8550", TicketSysId = "developer@sabreinfo.com.sg", TicketSysPassword = pass, contactEmail = "support@xyz.com" });

                //if (companyResult != null)
                //{
                //    var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "ZZTEST", TicketSysCompanyCode = "124", VerificationEmail = "abc@xyz.com", VerificationCode = "123456" });
                //    _jiraUserMgmtService.Create(new JiraUser { UserFbId = "3058942664196267", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
                //}

                //JObject message = JObject.FromObject(new
                //{
                //    recipient = new { id = "3419843058061019" },
                //    message = new
                //    {
                //        attachment = new
                //        {
                //            type = "template",
                //            payload = new
                //            {
                //                template_type = "one_time_notif_req",
                //                title = "Do you want to get notified with [ABC-001] updates?",
                //                payload = string.Format(FacebookCustomPayload.CASE_GET_NOTIFIED_PAYLOAD, "[ABC-001]")
                //            }
                //        }
                //    }
                //});

                //_fbApiClientService.PostMessageAsync(tempToken, message);

                //JObject message = JObject.FromObject(new
                //{
                //    recipient = new { one_time_notif_token = "6543617655184695474" },
                //    message = new
                //    {
                //        text = "Follow up:\n\n[ABC-001] updated to Completed"
                //    }
                //});
                //_fbApiClientService.PostMessageAsync(tempToken, message);


                //var errMsg = JObject.FromObject(new
                //{
                //    recipient = new { one_time_notif_token = "7915953949996185142" },
                //    message = new { text = $"Test Repeating Blast" }
                //});
                //await _fbApiClientService.PostMessageAsync("EAADcf5Tn8Q0BAA6DdKIRm8vQ1TuZCJA93pA893nGyZAabOsyNPJl7psqiEBZBtrrV318UBjecemc2quU3OkMQH8YGV6tR12tvZBMUzWaLuDWoell68ZB5YB0cWuvq0Phh5vFyS6av3vLqZCRR69Bdjhye05Ofs5zaI3sgfasd1Ukfkeg6LPbmuvBKtiGldx7kZD", errMsg);


                return Ok("Seed Complete.");
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger, this.Request, this.RouteData);
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("GetCaseDetail")]
        public IActionResult GetCaseDetail()
        {
            Company c = new Company()
            {
                TicketSysUrl = "http://58.185.112.2:8550",
                TicketSysId = "developer@sabreinfo.com.sg",
                TicketSysPassword = Utility.ParseEInfo("DevS@b3r", _appSettings.General.SysInfo)
        };

            return Ok(_caseMgmtService.GetCaseStatusAsync(c, "124", "ZZTST-1").Result);
        }

        [HttpPost]
        [Route("CreateCase")]
        public IActionResult CreateCase()
        {
            Company c = new Company()
            {
                TicketSysUrl = "http://58.185.112.2:8550",
                TicketSysId = "developer@sabreinfo.com.sg",
                TicketSysPassword = Utility.ParseEInfo("DevS@b3r", _appSettings.General.SysInfo)
            };

            ClientCompany cl = new ClientCompany()
            {
                TicketSysCompanyCode = "124"
            };

            return Ok(_caseMgmtService
                            .CreateCaseAsync(c, cl, $"Issue in test on {DateTime.Now.ToString("dd MMM yyyy")}",
                            "I have an issue in my system. please HELPPPPP!!!").Result);
        }

        [HttpPost]
        [Route("GetCompanyCodes")]
        public IActionResult GetCompanyCodes(string clientCompanyName)
        {
            Company c = new Company()
            {
                TicketSysUrl = "http://58.185.112.2:8550",
                TicketSysId = "developer@sabreinfo.com.sg",
                TicketSysPassword = Utility.ParseEInfo("DevS@b3r", _appSettings.General.SysInfo)
            };

            return Ok(_caseMgmtService.GetClientCompanies(c, clientCompanyName).Result);
        }
    }
}

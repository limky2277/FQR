using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Threading.Tasks;
using TicketBOT.Helpers;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;

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
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientCompanyService;

        private IConversationService _conversationService;

        public PlaygroundController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
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
                // Seed Data
                // var companyResult = _companyService.Create(new Company { CompanyName = "Sabre", FbPageId = "102327571503111", FbPageToken = "EAADcf5Tn8Q0BAA6DdKIRm8vQ1TuZCJA93pA893nGyZAabOsyNPJl7psqiEBZBtrrV318UBjecemc2quU3OkMQH8YGV6tR12tvZBMUzWaLuDWoell68ZB5YB0cWuvq0Phh5vFyS6av3vLqZCRR69Bdjhye05Ofs5zaI3sgfasd1Ukfkeg6LPbmuvBKtiGldx7kZD", TicketSysUrl= "http://58.185.112.2:8550", TicketSysId = "developer@sabreinfo.com.sg", TicketSysPassword = "DevS@b3r", contactEmail = "support@xyz.com" });

                //if (companyResult != null)
                //{
                //    var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "ZZTEST", TicketSysCompanyCode = "124", VerificationEmail = "abc@xyz.com", VerificationCode = "123456" });
                //    // _jiraUserMgmtService.Create(new JiraUser { UserFbId = "3058942664196267", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
                //}

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
            Company c = new Company() { TicketSysUrl = "http://58.185.112.2:8550",
             TicketSysId = "developer@sabreinfo.com.sg",
              TicketSysPassword = "DevS@b3r"
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
                TicketSysPassword = "DevS@b3r"
            };

            ClientCompany cl = new ClientCompany()
            {
                TicketSysCompanyCode = "124"
            };

            return Ok(_caseMgmtService
                            .CreateCaseAsync(c, cl, $"Issue in test on {DateTime.Now.ToString("dd MMM yyyy")}", 
                            "I have an issue in my system. please HELPPPPP!!!").Result) ;
        }

        [HttpPost]
        [Route("GetCompanyCodes")]
        public IActionResult GetCompanyCodes(string clientCompanyName)
        {
            Company c = new Company()
            {
                TicketSysUrl = "http://58.185.112.2:8550",
                TicketSysId = "developer@sabreinfo.com.sg",
                TicketSysPassword = "DevS@b3r"
            };

            return Ok(_caseMgmtService.GetClientCompanies(c, clientCompanyName).Result);
        }
    }
}

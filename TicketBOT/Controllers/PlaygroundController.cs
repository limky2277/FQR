using EasyCaching.Core;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Redis;
using Newtonsoft.Json;
using System;
using TicketBOT.Helpers;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;
using static TicketBOT.Models.QAConversation;

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

        // Redis
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly IEasyCachingProviderFactory _cachingProviderFactory;

        private ISenderCacheService _senderCacheService;

        public PlaygroundController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService,
            ClientCompanyService clientCompanyService, IEasyCachingProviderFactory cachingProviderFactory, 
            ISenderCacheService senderCacheService)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientCompanyService = clientCompanyService;
            _cachingProviderFactory = cachingProviderFactory;
            _cachingProvider = _cachingProviderFactory.GetCachingProvider(_appSettings.RedisSettings.CachingProvider);
            _senderCacheService = senderCacheService;
        }

        public IActionResult Get()
        {
            try
            {
                // Test error logs
                //int a = 1, b = 0;
                //var c = a / b;

                //_cachingProvider.Set("Key123", "Value1234", TimeSpan.FromMinutes(_appSettings.RedisSettings.TimeSpanMins));

                //var redisResult = _cachingProvider.Get<object>("Key123");

                //_senderCacheService.UpsertActiveConversation("3058942664196267", new QAConversation { FbSenderId = "3058942664196267", LastQuestionAsked = (int)Question.Issue, AnswerFreeText= "issue", Answered = true });

                var redisResult = _senderCacheService.GetConversationList("");

                var companyResult = _companyService.Create(new Company { CompanyName = "ABC Company", FbPageId = "", FbPageToken = "" });

                if (companyResult != null)
                {
                    var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "XYZ Client", VerificationEmail = "abc@xyz.com", VerificationCode = "123456" });
                    _jiraUserMgmtService.Create(new JiraUser { UserFbId = "3058942664196267", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
                }

                //return Ok(_jiraUserMgmtService.Get());
                return Ok(redisResult != null ? JsonConvert.SerializeObject(redisResult) : "Empty");
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger);
                return StatusCode(500);
            }
        }
    }
}

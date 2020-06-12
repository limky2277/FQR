using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Redis;
using Newtonsoft.Json;
using System;
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
        private readonly ApplicationSettings _appSettings;
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientCompanyService;
        private readonly ConversationService _conversationService;

        // Redis
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly IEasyCachingProviderFactory _cachingProviderFactory;

        private ISenderCacheService _senderCacheService;

        public PlaygroundController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService,
            ClientCompanyService clientCompanyService, ConversationService conversationService,
            IEasyCachingProviderFactory cachingProviderFactory, ISenderCacheService senderCacheService)
        {
            _appSettings = appSettings;
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientCompanyService = clientCompanyService;
            _conversationService = conversationService;
            _cachingProviderFactory = cachingProviderFactory;
            _cachingProvider = _cachingProviderFactory.GetCachingProvider(_appSettings.RedisSettings.CachingProvider);
            _senderCacheService = senderCacheService;
        }

        public IActionResult Get()
        {
            //_cachingProvider.Set("Key123", "Value1234", TimeSpan.FromMinutes(_appSettings.RedisSettings.TimeSpanMins));

            //var redisResult = _cachingProvider.Get<object>("Key123");

            //_senderCacheService.UpsertActiveConversation("3058942664196267", new QAConversation { FbSenderId = "3058942664196267", LastQuestionAsked = (int)Question.Issue, AnswerFreeText= "issue", Answered = true });

            var redisResult = _senderCacheService.GetConversationList("3058942664196267~103650171367830");

            var companyResult = _companyService.Create(new Company { CompanyName = "ABC Company", FbPageId = "103650171367830", FbPageToken = "EAAJHOBuXMzYBAPpVpakroPUU8YE8w6ZC57iya27Dd769N4sWjIbvQSatPnIv4NCO4MWdUqbXiBjp5y4hNwqV2W3Jfi2XyqsylkSQqw5vbDipkEUZCPMnEvcUNXL3xM7dfW4DCLOHnBMEwOZC7vpWGcriKrLROVp7JNRhUarP84VlphtKO2Edar3BhuFopsZD" });

            if (companyResult != null)
            {
                var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "XYZ Client", VerificationEmail = "abc@xyz.com", VerificationCode = "123456" });
                _conversationService.Create(new QAConversation { LastQuestionAsked = (int)QAConversation.Question.None });
                _jiraUserMgmtService.Create(new JiraUser { UserFbId = "3058942664196267", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
            }

            //return Ok(_jiraUserMgmtService.Get());
            return Ok(redisResult != null ? JsonConvert.SerializeObject(redisResult) : "Empty");
        }
    }
}

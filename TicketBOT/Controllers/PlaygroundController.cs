using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Redis;
using System;
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

        public PlaygroundController(ApplicationSettings appSettings, ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService, 
            ClientCompanyService clientCompanyService, ConversationService conversationService,
            IEasyCachingProviderFactory cachingProviderFactory)
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
        }

        public IActionResult Get()
        {
            //_cachingProvider.Set("Key123", "Value1234", TimeSpan.FromMinutes(_appSettings.RedisSettings.TimeSpanMins));

            //var redisResult = _cachingProvider.Get<object>("Key123");

            var companyResult = _companyService.Create(new Company { CompanyName = "ABC Company", FbPageId = "", FbPageToken = "" });

            if (companyResult != null)
            {
                var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "XYZ Client", VerificationEmail = "abc@xyz.com", VerificationCode = "123456" });
                _conversationService.Create(new QnAConversation { CompanyId = Guid.NewGuid(), FbSenderId = "ABC", LastQuestionAsked = (int)QnAConversation.Question.None });
                _jiraUserMgmtService.Create(new JiraUser { UserFbId = "", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
            }

            return Ok(_jiraUserMgmtService.Get());
            //return Ok(redisResult);
        }
    }
}

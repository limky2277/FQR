﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientCompanyService;
        private readonly ConversationService _conversationService;

        public PlaygroundController(ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService, 
            ClientCompanyService clientCompanyService, ConversationService conversationService)
        {
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientCompanyService = clientCompanyService;
            _conversationService = conversationService;
        }

        public IActionResult Get()
        {
            var companyResult = _companyService.Create(new Company { CompanyName = "ABC Company", FbPageId = "", FbPageToken = "" });

            if (companyResult != null)
            {
                var clientCompanyResult = _clientCompanyService.Create(new ClientCompany { ClientCompanyName = "XYZ Client" });
                _conversationService.Create(new QnAConversation { CompanyId = Guid.NewGuid(), FbSenderId = "ABC", LastQuestionAsked = (int)QnAConversation.Question.None });
                _jiraUserMgmtService.Create(new JiraUser { UserFbId = "", CompanyId = companyResult.Id, ClientCompanyId = clientCompanyResult.Id, UserNickname = "abc nickname" });
            }

            return Ok(_jiraUserMgmtService.Get());
        }
    }
}

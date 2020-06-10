using EasyCaching.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketBOT.Models;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;
using static TicketBOT.Models.Facebook.FacebookQuickReply;
using static TicketBOT.Models.QnAConversation;

namespace TicketBOT.BotAgent
{
    public class Bot
    {
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientService;
        private readonly ConversationService _conversationService;

        private FacebookSender _senderInfo;
        private Company _company;

        private ISenderCacheService _senderCacheService;

        public Bot(ICaseMgmtService caseMgmtService,
            JiraUserMgmtService jiraUserMgmtService, IFbApiClientService fbApiClientService,
            CompanyService companyService, ClientCompanyService clientService,
            ConversationService conversationService, ISenderCacheService senderCacheService)
        {
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientService = clientService;
            _conversationService = conversationService;
            _senderCacheService = senderCacheService;
        }

        public async Task<bool> DispatchAgent(Messaging incomingMessage, Company company)
        {
            //read user name here. Return null if user not found
            _senderInfo = await _fbApiClientService.GetUserInfoAsync(company.FbPageToken, incomingMessage.sender.id);
            _senderInfo.senderConversationId = incomingMessage.sender.id;
            _company = company;

            // INIT.1.0 Consider Redis before proceed A.1.0
            // INIT.1.1 Check any active conversation (Past x mins) To break the conversation loop

            // If no active conversation, send greeting
            if (!_senderCacheService.AnyActiveConversation(_senderInfo.senderConversationId))
            {
                // A.1.0
                // Send greeting first
                // E.g.: Hi there, how can I help you? + Quick Reply Button(Raise a ticket / Check ticket status)
                await ConstructAndSendMessage(ConstructType.Greeting);
            }
            else
            {
                // Check Payload
                // Is there quick reply buttons associated
                // If quick reply button associated: To-do
                var anyQuickReply = await CheckQuickReplyPayload(incomingMessage);

                if (anyQuickReply)
                {
                    // If no quick reply button associated 
                    // Check db for previous conversation SELECT TOP 1 * FROM Conversation WHERE CompanyId = X AND FbSenderId = X AND ModifiedOn DESC
                    // If record found --> extract Conversation.Answered
                    var lastQuestion = _conversationService.GetLastQuestion(_company.Id, _senderInfo.id);
                    if (lastQuestion != null)
                    {
                        if (!lastQuestion.Answered)
                        {
                            // If answer not fullfilled, ask/ask again
                            // Extract Conversation.LastQuestionAsked
                            // extract message from payload. Validate input
                            ValidateQnA(lastQuestion, "answer");
                        }
                        else
                        {
                            // If answer fullfilled, prepare next question

                            // Conversation.LastQuestionAsked, Conversation.AnswerFreeText
                        }
                    }
                }

            }

            return false;
        }

        private async Task<bool> CheckQuickReplyPayload(Messaging incomingMessage)
        {
            var payload = "NONE";
            try
            {
                payload = incomingMessage.message.quick_reply.payload;
            }
            catch { }

            switch (payload)
            {
                case RAISE_TICKET:
                    PrepareRaiseTicket();
                    return true;
                case TICKET_STATUS:
                    return true;
                default:
                    await ConstructAndSendMessage(ConstructType.Ending);
                    return false;
            }
        }

        private void PrepareRaiseTicket()
        {
            // Check is user registered
            // Extract senderInfo.senderId, company.Id
            // Retrieve from user where UserFbId = x, CompanyId = x, Active = true
            var user = _jiraUserMgmtService.GetUser(_senderInfo.id, _company.Id);
            if (user != null)
            {
                // If user found, prepare create ticket session

                // Please raise your ticket in the next message
                // Inform template 
                // Insert into conversation db ConversationFlow.Question.Ticket
                // Send question to sender, ask for ticket description
            }
            else
            {
                // If user not found, register user

                // Seems like you are not registered. 
                // Request user to register now
                // Insert into conversation db ConversationFlow.Question.Company
                // Send question to sender, ask for company
            }
        }

        private void ValidateQnA(QnAConversation conversation, string answer)
        {
            switch (conversation.LastQuestionAsked)
            {
                case (int)Question.Company:
                    bool isAnswerValid = false; // Validate input from message (answer) 
                    if (!isAnswerValid)
                    {
                        // Invalid input / no company found, please try again
                    }
                    else
                    {
                        // Search clients databases
                        var clientList = _companyService.Get();
                        var clientResult = clientList.Where(x => x.CompanyName == answer).FirstOrDefault();

                        if (clientResult != null)
                        {
                            // Update conversation
                            conversation.Answered = true;
                            conversation.AnswerFreeText = answer;
                            conversation.ModifiedOn = DateTime.Now;

                            // Register user
                            JiraUser user = new JiraUser { UserFbId = _senderInfo.id, ClientCompanyId = clientResult.Id, CompanyId = _company.Id, UserNickname = $"{_senderInfo.last_name} {_senderInfo.first_name}" };

                            _jiraUserMgmtService.Create(user);

                            // Prepare next question (Conversation.Question.Issue) and send to sender
                            // What is your issue?
                        }
                        else
                        {
                            // Invalid input / no company found, please try again
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        #region Prepare Quick Reply Buttons
        /// <summary>
        /// get text message template
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="sender">sender id</param>
        /// <returns>json</returns>
        private async Task<List<JObject>> ConstructAndSendMessage(ConstructType type)
        {
            // To-do: consider to convert to JSON 
            List<JObject> messageList = new List<JObject>();

            switch (type)
            {
                case ConstructType.Greeting:
                    var greetingOption = new List<QuickReplyOption>
                    {
                        new QuickReplyOption { title = RAISE_TICKET, payload = RAISE_TICKET },
                        new QuickReplyOption { title = TICKET_STATUS, payload = TICKET_STATUS },
                        new QuickReplyOption { title = JUST_BROWSE, payload = JUST_BROWSE },
                    };

                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Greeting {_senderInfo.last_name}! I'm TicketBOT! We love having you with us." }
                    }));

                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"How can I help you? Here some option(s).", quick_replies = greetingOption }
                    }));

                    _senderCacheService.UpsertActiveConversation(_senderInfo.senderConversationId, DateTime.Now.ToString());
                    break;
                case ConstructType.Ending:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Thank you! Have a nice day! :)." }
                    }));

                    _senderCacheService.RemoveActiveConversation(_senderInfo.senderConversationId);
                    break;
                case ConstructType.None:
                    break;

            }

            foreach (var message in messageList)
            {
                await _fbApiClientService.PostMessageAsync(_company.FbPageToken, message);
            }
            return messageList;
        }
        #endregion
    }
}

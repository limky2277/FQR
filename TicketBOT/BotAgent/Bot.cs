using System;
using System.Linq;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;

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

        private SenderInfo _senderInfo;
        private Company _company;

        public Bot(ICaseMgmtService caseMgmtService, JiraUserMgmtService jiraUserMgmtService,
            IFbApiClientService fbApiClientService, CompanyService companyService, ClientCompanyService clientService, ConversationService conversationService)
        {
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientService = clientService;
            _conversationService = conversationService;
        }

        public bool DispatchAgent(SenderInfo senderInfo, Company company)
        {
            _senderInfo = senderInfo;
            _company = company;

            // INIT.1.0 Consider Redis before proceed A.1.0
            // INIT.1.1 Check any active conversation (Past x mins) To break the conversation loop

            // If no active conversation, send greeting
            bool anyActiveConver = false;
            if (anyActiveConver)
            {
                // A.1.0
                // Send greeting first
                // E.g.: Hi there, how can I help you? + Quick Reply Button(Raise a ticket / Check ticket status)
                SendGreeting();
            }
            else
            {
                // Check Payload
                // Is there quick reply buttons associated
                // If quick reply button associated: To-do
                var anyQuickReply = CheckQuickReplyPayload("");

                if (!anyQuickReply)
                {
                    // If no quick reply button associated 
                    // Check db for previous conversation SELECT TOP 1 * FROM Conversation WHERE CompanyId = X AND FbSenderId = X AND ModifiedOn DESC
                    // If record found --> extract Conversation.Answered
                    var lastQuestion = _conversationService.GetLastQuestion(_company.Id, _senderInfo.id);

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

            return false;
        }

        public bool CheckQuickReplyPayload(string payload)
        {
            switch (payload)
            {
                case "Raise a ticket":
                    PrepareRaiseTicket();
                    return true;
                case "Check ticket status":
                    return true;
                default:
                    return false;
            }
        }

        public void SendGreeting()
        {

        }

        public void PrepareRaiseTicket()
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

        public void ValidateQnA(QnAConversation conversation, string answer)
        {
            switch (conversation.LastQuestionAsked)
            {
                case (int)QnAConversation.Question.Company:
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
    }
}

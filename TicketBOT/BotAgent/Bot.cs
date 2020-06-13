using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketBOT.Helpers;
using TicketBOT.Models;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;
using static TicketBOT.Models.Facebook.FacebookQuickReply;
using static TicketBOT.Models.QAConversation;

namespace TicketBOT.BotAgent
{
    public class Bot
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly JiraUserMgmtService _jiraUserMgmtService;
        private readonly CompanyService _companyService;
        private readonly ClientCompanyService _clientService;

        private FacebookSender _senderInfo;
        private Company _company;

        private ISenderCacheService _senderCacheService;

        public Bot(ICaseMgmtService caseMgmtService,
            JiraUserMgmtService jiraUserMgmtService, IFbApiClientService fbApiClientService,
            CompanyService companyService, ClientCompanyService clientService,
            ISenderCacheService senderCacheService)
        {
            _caseMgmtService = caseMgmtService;
            _jiraUserMgmtService = jiraUserMgmtService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _clientService = clientService;
            _senderCacheService = senderCacheService;
        }

        public async Task DispatchAgent(Messaging incomingMessage, Company company)
        {
            try
            {
                //read user name here. Return null if user not found
                _senderInfo = await _fbApiClientService.GetUserInfoAsync(company.FbPageToken, incomingMessage.sender.id);
                _senderInfo.senderConversationId = incomingMessage.sender.id;
                _company = company;

                // INIT.1.0 Consider Redis before proceed A.1.0
                // INIT.1.1 Check any active conversation (Past x mins) To break the conversation loop

                // If no active conversation, send greeting
                if (!_senderCacheService.AnyActiveConversation($"{_senderInfo.senderConversationId}~{company.FbPageId}"))
                {
                    // A.1.0
                    // Send greeting first
                    // E.g.: Hi there, how can I help you? + Quick Reply Button(Raise a ticket / Check ticket status)
                    await ConstructAndSendMessage(ConstructType.Greeting);
                }
                else
                {
                    // If no quick reply button associated 
                    // Check db for previous conversation SELECT TOP 1 * FROM Conversation WHERE CompanyId = X AND FbSenderId = X AND ModifiedOn DESC
                    // If record found --> extract Conversation.Answered
                    // var lastQuestion = _conversationService.GetLastQuestion(_company.Id, _senderInfo.id);
                    var lastQuestion = _senderCacheService.LastConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");

                    if (lastQuestion != null)
                    {
                        if (!lastQuestion.Answered)
                        {
                            // If retry option = true
                            // Delete retry option from cache and roll back to conversation before retry

                            // If answer not fullfilled, ask/ask again
                            // Extract Conversation.LastQuestionAsked
                            // extract message from payload. Validate input
                            await ValidateQnA(lastQuestion, incomingMessage.message.text);
                        }
                        else
                        {
                            // If answer fullfilled, prepare next question

                            // Check Payload
                            // Is there quick reply buttons associated
                            // If quick reply button associated: To-do
                            var anyQuickReply = CheckQuickReplyPayload(incomingMessage);
                            if (!string.IsNullOrEmpty(anyQuickReply))
                            {
                                // Conversation.LastQuestionAsked, Conversation.AnswerFreeText
                                switch (anyQuickReply)
                                {
                                    case RAISE_TICKET:
                                        await PrepareRaiseTicket();
                                        break;
                                    case TICKET_STATUS:
                                        await PrepareCheckTicketStatus();
                                        break;
                                    case RETRY_YES:
                                        _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                                        await ConstructAndSendMessage(ConstructType.Greeting);
                                        break;
                                    case RETRY_NO:
                                        await ConstructAndSendMessage(ConstructType.Ending);
                                        break;
                                    default:
                                        // prompt: Apologize due to not recognize selected option. Send relevant / available option again
                                        await ConstructAndSendMessage(ConstructType.NotImplemented);
                                        break;
                                }
                            }
                            else
                            {
                                await ConstructAndSendMessage(ConstructType.NotImplemented);
                            }
                        }
                    }
                    else
                    {
                        await ConstructAndSendMessage(ConstructType.Greeting);
                    }
                }
            }
            catch (Exception ex)
            {
                await ConstructAndSendMessage(ConstructType.Error);
                LoggingHelper.LogError(ex, _logger);
            }
        }

        #region Prepare Question for Sender
        private async Task PrepareRaiseTicket()
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
                await ConstructAndSendMessage(ConstructType.CreateTicket);
            }
            else
            {
                // If user not found, register user

                // Seems like you are not registered. 
                // Request user to register now
                // Insert into conversation db ConversationFlow.Question.Company
                // Send question to sender, ask for company
                await ConstructAndSendMessage(ConstructType.SearchCompany);
            }
        }

        private async Task PrepareCheckTicketStatus()
        {
            // Jira integration
            bool ticketStatus = true;
            if (ticketStatus)
            {
                await ConstructAndSendMessage(ConstructType.CheckTicket);
            }
            else
            {
                // Ticket not found 
                // Send apologize message

                // Reset to greeting
                // await ConstructAndSendMessage(ConstructType.Greeting)
                await ConstructAndSendMessage(ConstructType.NotImplemented);
            }
        }
        #endregion 

        #region Validate QnA
        private async Task ValidateQnA(QAConversation conversation, string answer)
        {
            switch (conversation.LastQuestionAsked)
            {
                case (int)Question.Company:
                    // Validate message (reported issue) e.g. min length
                    if (!string.IsNullOrEmpty(answer))
                    {
                        // Search clients databases
                        var clientList = _clientService.Get();
                        var clientResult = clientList.Where(x => x.ClientCompanyName.ToLower() == answer.ToLower()).FirstOrDefault();

                        if (clientResult != null)
                        {
                            conversation.Answered = true;
                            conversation.AnswerFreeText = clientResult.Id.ToString();
                            conversation.ModifiedOn = DateTime.Now;
                            _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                            // Ask for verification code
                            await ConstructAndSendMessage(ConstructType.RequestVerificationCode);
                        }
                        else
                        {
                            conversation.ModifiedOn = DateTime.Now;
                            _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                            // Invalid input / no company found, please try again
                            await ConstructAndSendMessage(ConstructType.Retry);
                        }
                    }
                    else
                    {
                        conversation.ModifiedOn = DateTime.Now;
                        _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                        // Invalid input / no company found, please try again
                        await ConstructAndSendMessage(ConstructType.Retry);
                    }
                    break;
                case (int)Question.VerificationCode:
                    // Validate message (reported issue) e.g. min length
                    if (!string.IsNullOrEmpty(answer))
                    {
                        // Check previous conversation, get ClientCompanyGuid
                        List<QAConversation> qaConv = _senderCacheService.GetConversationList($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                        if (qaConv != null)
                        {
                            QAConversation verifyConv = qaConv.Where(x => x.LastQuestionAsked == (int)Question.Company).FirstOrDefault();

                            // Check verification code
                            var clientList = _clientService.Get();
                            var clientResult = clientList.Where(x => x.Id == Guid.Parse(verifyConv.AnswerFreeText) && x.VerificationCode == answer).FirstOrDefault();

                            if (clientResult != null)
                            {
                                // Register user
                                JiraUser user = new JiraUser { UserFbId = _senderInfo.id, ClientCompanyId = clientResult.Id, CompanyId = _company.Id, UserNickname = $"{_senderInfo.last_name} {_senderInfo.first_name}" };

                                _jiraUserMgmtService.Create(user);

                                // Begin create ticket
                                await ConstructAndSendMessage(ConstructType.CreateTicket);
                            }
                            else
                            {
                                // Incorrect verification code. Try again or end conversation
                                conversation.ModifiedOn = DateTime.Now;
                                _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                                // Invalid input / no company found, please try again
                                await ConstructAndSendMessage(ConstructType.Retry);
                            }
                        }
                        else
                        {
                            conversation.ModifiedOn = DateTime.Now;
                            _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                            // Invalid input / no company found, please try again
                            await ConstructAndSendMessage(ConstructType.Retry);
                        }
                    }
                    else
                    {
                        await ConstructAndSendMessage(ConstructType.NotImplemented);
                    }
                    break;
                case (int)Question.Issue:
                    // Validate message (reported issue) e.g. min length
                    if (!string.IsNullOrEmpty(answer))
                    {
                        conversation.Answered = true;
                        conversation.AnswerFreeText = answer;
                        conversation.ModifiedOn = DateTime.Now;
                        _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                        // Jira integration here

                        // Inform user we have loggeed your case, please quote [case_num].....
                        await ConstructAndSendMessage(ConstructType.TicketCreated);

                    }
                    else
                    {
                        conversation.ModifiedOn = DateTime.Now;
                        _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                        // Invalid input / no company found, please try again
                        await ConstructAndSendMessage(ConstructType.Retry);
                    }
                    break;
                case (int)Question.TicketCode:
                    // Validate message (reported issue) e.g. min length
                    if (!string.IsNullOrEmpty(answer))
                    {
                        // Jira integration here
                        // Search ticket status
                        bool ticketFound = true;
                        if (ticketFound)
                        {
                            conversation.Answered = true;
                            conversation.AnswerFreeText = answer;
                            conversation.ModifiedOn = DateTime.Now;
                            _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                            await ConstructAndSendMessage(ConstructType.TicketFound);
                        }
                        else
                        {
                            conversation.ModifiedOn = DateTime.Now;
                            _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                            // Invalid input / no company found, please try again
                            await ConstructAndSendMessage(ConstructType.Retry);
                        }
                    }
                    else
                    {
                        conversation.ModifiedOn = DateTime.Now;
                        _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", conversation);

                        // Invalid input / no company found, please try again
                        await ConstructAndSendMessage(ConstructType.Retry);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Prepare Quick Reply Buttons + Construct Message, Send Message & Update Cache
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

                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.None, Answered = true });
                    break;
                case ConstructType.Ending:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Thank you! Have a nice day! :)." }
                    }));

                    _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                    break;
                case ConstructType.CreateTicket:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Okay got it! Please tell me about your issue(s)." }
                    }));
                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.Issue, Answered = false });
                    break;
                case ConstructType.TicketCreated:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"All done! Your case has been logged. Please quote [case_num] to follow up." }
                    }));
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Thank you for using TicketBOT! Have a nice day! :)." }
                    }));
                    _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                    break;
                case ConstructType.CheckTicket:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Sure! Please quote your ticket code." }
                    }));
                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.TicketCode, Answered = false });
                    break;
                case ConstructType.TicketFound:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"There you go! [ticket_status]" }
                    }));
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Thank you for using TicketBOT! Have a nice day! :)." }
                    }));
                    _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                    break;
                case ConstructType.SearchCompany:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Before we get started, I wanna know one thing. Can you tell me your company name please?" }
                    }));
                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.Company, Answered = false });
                    break;
                case ConstructType.RequestVerificationCode:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Okay great! We've sent you a verification email which contains a verification code. Can you tell me your verification code please?" }
                    }));
                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.VerificationCode, Answered = false });
                    break;
                case ConstructType.Retry:
                    var retryOption = new List<QuickReplyOption>
                    {
                        new QuickReplyOption { title = RETRY_YES, payload = RETRY_YES },
                        new QuickReplyOption { title = RETRY_NO, payload = RETRY_NO },
                    };

                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Sorry, I didn't quite catch that. It seems like invalid option/answer." }
                    }));
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"Do you want to retry?", quick_replies = retryOption }
                    }));
                    _senderCacheService.UpsertActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}", new QAConversation { LastQuestionAsked = (int)Question.Retry, Answered = true });
                    break;
                case ConstructType.NotImplemented:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"DEBUG --> Not implemented." }
                    }));
                    _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
                    break;
                case ConstructType.Error:
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { id = _senderInfo.senderConversationId },
                        message = new { text = $"DEBUG --> Error. Check exception" }
                    }));
                    _senderCacheService.RemoveActiveConversation($"{_senderInfo.senderConversationId}~{_company.FbPageId}");
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

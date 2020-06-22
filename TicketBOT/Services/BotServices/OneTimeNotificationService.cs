using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketBOT.Core.Models;
using TicketBOT.Helpers;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.DBServices;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.BotServices
{
    public class OneTimeNotificationService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TicketSysUserMgmtService _jiraUserMgmtService;
        private Company _company;
        private readonly UserCaseNotifService _userCaseNotifService;
        private readonly IFbApiClientService _fbApiClientService;


        public OneTimeNotificationService(TicketSysUserMgmtService jiraUserMgmtService, UserCaseNotifService userCaseNotifService, IFbApiClientService fbApiClientService)
        {
            _jiraUserMgmtService = jiraUserMgmtService;
            _userCaseNotifService = userCaseNotifService;
            _fbApiClientService = fbApiClientService;
        }

        public async Task UserOptinCaseNoification(Messaging message, Company company)
        {
            try
            {
                _company = company;

                var jiraCaseKey = message.optin.payload.Replace(FacebookCustomPayload.CASE_GET_NOTIFIED, string.Empty);
                var oneTimeNotifToken = message.optin.one_time_notif_token;

                var user = _jiraUserMgmtService.GetUser(message.sender.id, _company.Id);

                TicketSysNotification ticketSysNotification = new TicketSysNotification
                {
                    TicketSysUserId = user.Id,
                    OneTimeNotifToken = oneTimeNotifToken,
                    JiraCaseKey = jiraCaseKey,
                };

                _userCaseNotifService.Create(ticketSysNotification);

                List<JObject> messageList = new List<JObject>();

                messageList.Add(JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"All set! We'll send you a notification when there is an update!" }
                }));
                messageList.Add(JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"Thank you for using TicketBOT! Have a nice day! :)." }
                }));

                foreach (var msg in messageList)
                {
                    await _fbApiClientService.PostMessageAsync(_company.FbPageToken, msg);
                }
            }
            catch (Exception ex)
            {
                var errMsg = JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"DEBUG --> Error. Check exception" }
                });
                await _fbApiClientService.PostMessageAsync(_company.FbPageToken, errMsg);
                LoggingHelper.LogError(ex, _logger);
            }
        }
    }
}

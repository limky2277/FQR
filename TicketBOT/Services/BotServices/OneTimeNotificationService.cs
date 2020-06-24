using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.Helpers;
using TicketBOT.JIRA.Models;
using TicketBOT.JIRA.Services;
using TicketBOT.Models.Facebook;
using TicketBOT.Services.DBServices;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.BotServices
{
    public class OneTimeNotificationService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IFbApiClientService _fbApiClientService;
        private readonly TicketSysUserMgmtService _jiraUserMgmtService;
        private readonly ITicketSysNotificationService _userCaseNotifService;
        private Company _company;
        private readonly CompanyService _companyService;
        private readonly ICaseMgmtService _jiraCaseMgmtService;
        private readonly ClientCompanyService _clientCompanyService;
        private readonly ApplicationSettings _applicationSettings;

        public OneTimeNotificationService(TicketSysUserMgmtService jiraUserMgmtService, ITicketSysNotificationService userCaseNotifService,
            IFbApiClientService fbApiClientService, CompanyService companyService, ICaseMgmtService jiraCaseMgmtService,
            ClientCompanyService clientCompanyService, ApplicationSettings applicationSettings)
        {
            _jiraUserMgmtService = jiraUserMgmtService;
            _userCaseNotifService = userCaseNotifService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
            _jiraCaseMgmtService = jiraCaseMgmtService;
            _clientCompanyService = clientCompanyService;
            _applicationSettings = applicationSettings;
        }

        public async Task UserOptinCaseNoification(Messaging message, Company company)
        {
            try
            {
                _company = company;

                var jiraCaseKey = message.optin.payload.Replace(FacebookCustomPayload.CASE_GET_NOTIFIED, string.Empty);
                var oneTimeNotifToken = message.optin.one_time_notif_token;

                var user = _jiraUserMgmtService.GetUser(message.sender.id, _company.Id);
                var ticketUserCompany = _clientCompanyService.GetById(user.ClientCompanyId);

                // Get current ticket status
                CaseDetail caseDetail = null;
                try
                {
                    caseDetail = await _jiraCaseMgmtService.GetCaseStatusAsync(company, ticketUserCompany.TicketSysCompanyCode, jiraCaseKey);
                }
                catch { }

                // Jira no result

                TicketSysNotification ticketSysNotification = new TicketSysNotification
                {
                    TicketSysUserId = user.Id,
                    OneTimeNotifToken = oneTimeNotifToken,
                    JiraCaseKey = jiraCaseKey,
                    JiraCaseStatus = caseDetail != null ? caseDetail.Status : null,
                };

                _userCaseNotifService.Create(ticketSysNotification);

                List<JObject> messageList = new List<JObject>();

                messageList.Add(JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"All set! We'll send you a notification when there is an update! 👍" }
                }));
                messageList.Add(JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"Thank you for using TicketBOT! Have a nice day! :)." }
                }));

                foreach (var msg in messageList)
                {
                    await _fbApiClientService.PostMessageAsync(Utility.ParseDInfo(_company.FbPageToken, _applicationSettings.General.SysInfo), msg);
                }
            }
            catch (Exception ex)
            {
                var errMsg = JObject.FromObject(new
                {
                    recipient = new { id = message.sender.id },
                    message = new { text = $"DEBUG --> Error. Check exception" }
                });
                await _fbApiClientService.PostMessageAsync(Utility.ParseDInfo(_company.FbPageToken, _applicationSettings.General.SysInfo), errMsg);
                LoggingHelper.LogError(ex, _logger);
            }
        }

        public async Task BlastJiraStatusUpdateNotification()
        {
            try
            {
                List<TicketSysNotification> pendingStatusUpdateNotifList = _userCaseNotifService.Get();

                foreach (var pending in pendingStatusUpdateNotifList)
                {
                    var ticketUser = _jiraUserMgmtService.GetById(pending.TicketSysUserId);
                    var ticketUserCompany = _clientCompanyService.GetById(ticketUser.ClientCompanyId);
                    _company = _companyService.GetById(ticketUser.CompanyId);

                    CaseDetail caseDetail = null;
                    try
                    {
                        caseDetail = await _jiraCaseMgmtService.GetCaseStatusAsync(_company, ticketUserCompany.TicketSysCompanyCode, pending.JiraCaseKey);
                    }
                    catch { }

                    // Jira no result
                    if (caseDetail == null) { continue; }

                    // If Jira status remain the same / no updates
                    var pendingStatus = pending.JiraCaseStatus ?? "";
                    if (caseDetail.Status.Trim().ToLower().Equals(pendingStatus.Trim().ToLower())) { continue; }

                    // Prepare send notification
                    List<JObject> messageList = new List<JObject>();
                    messageList.Add(JObject.FromObject(new
                    {
                        recipient = new { one_time_notif_token = pending.OneTimeNotifToken },
                        message = new { text = $"New Updates with Case: {caseDetail.CaseKey} \n\nStatus changed to: {caseDetail.Status} \n\nCase Subject: {caseDetail.Subject} \n\nClick the link below for more. \n{caseDetail.WebURL}" }
                    }));

                    // Delete entry from database
                    _userCaseNotifService.Remove(pending);

                    // If case status not completed, then ask whether want to subscribe for next update or not
                    if (caseDetail.Status != JiraServiceDeskStatus.Declined || caseDetail.Status != JiraServiceDeskStatus.Completed)
                    {
                        messageList.Add(JObject.FromObject(new
                        {
                            recipient = new { id = ticketUser.UserFbId },
                            message = new
                            {
                                attachment = new
                                {
                                    type = "template",
                                    payload = new
                                    {
                                        template_type = "one_time_notif_req",
                                        title = $"Do you want to get notified with {pending.JiraCaseKey} updates?",
                                        payload = string.Format(FacebookCustomPayload.CASE_GET_NOTIFIED_PAYLOAD, pending.JiraCaseKey)
                                    }
                                }
                            }
                        }));
                    }

                    foreach (var msg in messageList)
                    {
                        await _fbApiClientService.PostMessageAsync(Utility.ParseDInfo(_company.FbPageToken, _applicationSettings.General.SysInfo), msg);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger);
                throw;
            }
        }
    }
}

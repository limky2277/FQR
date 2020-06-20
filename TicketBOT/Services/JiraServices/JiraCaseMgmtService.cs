using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicketBOT.Helpers;
using TicketBOT.Models;
using TicketBOT.Models.JIRA;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class JiraCaseMgmtService : ICaseMgmtService
    {        
        public ApplicationSettings _appSettings { get; set; }
        public string _Auth { get; set; }
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JiraCaseMgmtService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<Models.CaseDetail> CreateCaseAsync(Company company, ClientCompany clientCompany, string CaseSubject, string CaseDescription)
        {
            HttpResponseMessage resp = null;
            try
            {
                Models.CaseDetail caseDetail = new Models.CaseDetail();
                if (company == null)
                    throw new ArgumentException($"Company details should not be empty", "Company");
                if (string.IsNullOrWhiteSpace(CaseSubject))
                    throw new ArgumentException($"Please enter correct Case subject", "CaseSubject");

                var JIRAReq = getJIRARequestObj(company.TicketSysId, company.TicketSysPassword);
                JIRAReq.Method = HttpMethod.Post;
                JIRAReq.RequestUri = new Uri(string.Format(_appSettings.JIRAApiEndpoint.CreateCase, company.TicketSysUrl));
                var caseDt = new ServiceDeskRequest()
                {
                    serviceDeskId = clientCompany.TicketSysCompanyCode,
                    requestTypeId = "1240",
                    requestFieldValues = new RequestFieldValues() { 
                        summary = CaseSubject, 
                        description = CaseDescription,
                        duedate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd")
                    },
                       
                };

                JIRAReq.Content = new StringContent(
                                                    JsonConvert.SerializeObject(caseDt),
                                                    Encoding.UTF8,
                                                    "application/json");

                resp = await RestApiHelper.SendAsync(JIRAReq);
                resp.EnsureSuccessStatusCode();
                var caseInfo = JsonConvert.DeserializeObject<Models.JIRA.JIRACaseDetail>(await resp.Content.ReadAsStringAsync());

                caseDetail = new Models.CaseDetail()
                {
                    CaseID = caseInfo.issueId,
                    CaseKey = caseInfo.issueKey,
                    CreatedOn = caseInfo.createdDate.jira,
                    Status = caseInfo.currentStatus.status,
                    Subject = caseInfo.requestFieldValues.Where(x => x.fieldId == "summary").FirstOrDefault()?.value.ToString(),
                    Detail = caseInfo.requestFieldValues.Where(x => x.fieldId == "description").FirstOrDefault()?.value.ToString(),
                    WebURL = caseInfo.Links.Web
                };

                return caseDetail;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                string APIError = "<EMPTY>";
                if (resp != null)
                { 
                    var error = JsonConvert.DeserializeObject<Error>(await resp.Content.ReadAsStringAsync());
                    if (error != null)
                        APIError = error.errorMessage;
                }

                throw new Exception($"Error: {ex.Message}. API Error:{APIError}");
            }

        }

        public async Task<Models.CaseDetail> GetCaseStatusAsync(Company company, string TicketSysCompanyCode, string CaseId)
        {
            HttpResponseMessage resp = null;
            try
            {
                Models.CaseDetail caseDetail = new Models.CaseDetail();
                if (string.IsNullOrWhiteSpace(TicketSysCompanyCode))
                    throw new ArgumentException($"Please enter correct company code", "TicketSysCompanyCode");
                if (string.IsNullOrWhiteSpace(CaseId))
                    throw new ArgumentException($"Please enter correct CaseId", "CaseId");

                var JIRAReq = getJIRARequestObj(company.TicketSysId, company.TicketSysPassword);
                JIRAReq.Method = HttpMethod.Get;
                JIRAReq.RequestUri = new Uri(string.Format(_appSettings.JIRAApiEndpoint.GetStatus, company.TicketSysUrl, CaseId));

                resp = await RestApiHelper.SendAsync(JIRAReq);
                resp.EnsureSuccessStatusCode();
                var caseInfo = JsonConvert.DeserializeObject<Models.JIRA.JIRACaseDetail>(await resp.Content.ReadAsStringAsync());

                if (!caseInfo.serviceDeskId.ToString().Equals(TicketSysCompanyCode))
                    throw new ArgumentException($"This case ID {caseInfo.issueKey} doesnt link to your service desk. Please try again", "CaseId");

                caseDetail = new Models.CaseDetail()
                {
                    CaseID = caseInfo.issueId,
                    CaseKey = caseInfo.issueKey,
                    CreatedOn = caseInfo.createdDate.jira,
                    Status = caseInfo.currentStatus.status,
                    Subject = caseInfo.requestFieldValues.Where(x => x.fieldId == "summary").FirstOrDefault()?.value.ToString(),
                    Detail = caseInfo.requestFieldValues.Where(x => x.fieldId == "description").FirstOrDefault()?.value.ToString(),
                    WebURL = caseInfo.Links.Web
                };

                return caseDetail;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                string APIError = "<EMPTY>";
                if (resp != null)
                {
                    var error = JsonConvert.DeserializeObject<Error>(await resp.Content.ReadAsStringAsync());
                    if (error != null)
                        APIError = error.errorMessage;
                }

                throw new Exception($"Error: {ex.Message}. API Error:{APIError}");
            }
        }

        /// <summary>
        /// This helps to get a request object for JIRA with the auth header added.
        /// Once you receive the obj, please set the method, request URI and content
        /// var JIRAReq = getJIRARequestObj();
        /// JIRAReq.RequestUri = new Uri("URL HERE");
        /// JIRAReq.Method = HttpMethod.Get;
        /// JIRAReq.Content = new StringContent(JsonConvert.SerializeObject(objectBody)); 
        /// </summary>
        /// <returns></returns>
        private HttpRequestMessage getJIRARequestObj(string UserID, string Pass) 
        {
            string authDetail = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{UserID}:{Pass}"));
            return new HttpRequestMessage
            {
                Headers = {
                { HttpRequestHeader.Authorization.ToString(), $"Basic {authDetail}" }}
            };
        }

       

       
    }
}

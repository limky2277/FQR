using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public JiraCaseMgmtService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task<Models.CaseDetail> CreateCaseAsync(Company company, string CaseSubject, string CaseDescription)
        {
            throw new NotImplementedException();
        }

        public async Task<Models.CaseDetail> GetCaseStatusAsync(Company company, string TicketSysCompanyCode, string CaseId)
        {
            Models.CaseDetail caseDetail = new Models.CaseDetail();
            if (string.IsNullOrWhiteSpace(TicketSysCompanyCode))
                throw new ArgumentException($"Please enter correct company code", "TicketSysCompanyCode");
            if (string.IsNullOrWhiteSpace(CaseId))
                throw new ArgumentException($"Please enter correct CaseId", "CaseId");

            var JIRAReq = getJIRARequestObj(company.TicketSysId, company.TicketSysPassword);
            JIRAReq.Method = HttpMethod.Get;
            JIRAReq.RequestUri = new Uri(string.Format(_appSettings.JIRAApiEndpoint.GetStatus, company.TicketSysUrl, CaseId));

            var resp = await RestApiHelper.SendAsync(JIRAReq);
            var caseInfo = JsonConvert.DeserializeObject<Models.JIRA.CaseDetail>(await resp.Content.ReadAsStringAsync());
            
            if (! caseInfo.serviceDeskId.ToString().Equals(TicketSysCompanyCode) )
                throw new ArgumentException($"This case ID {caseInfo.issueKey} doesnt link to your service desk. Please try again", "CaseId");

            caseDetail = new Models.CaseDetail() {
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

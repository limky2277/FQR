using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.JIRA.Models;

namespace TicketBOT.JIRA.Services
{
    public class JiraCaseMgmtService : ICaseMgmtService
    {
        private ApplicationSettings _appSettings { get; set; }
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JiraCaseMgmtService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Used to create a new case in JIRA
        /// </summary>
        /// <param name="company"></param>
        /// <param name="clientCompany"></param>
        /// <param name="CaseSubject"></param>
        /// <param name="CaseDescription"></param>
        /// <returns></returns>
        public async Task<CaseDetail> CreateCaseAsync(Company company, ClientCompany clientCompany, string CaseSubject, string CaseDescription)
        {
            HttpResponseMessage resp = null;
            try
            {
                CaseDetail caseDetail = new CaseDetail();
                if (company == null)
                    throw new ArgumentException($"Company details should not be empty", "Company");
                if (string.IsNullOrWhiteSpace(CaseSubject))
                    throw new ArgumentException($"Please enter correct Case subject", "CaseSubject");

                var JIRAReq = getJIRARequestObj(company.TicketSysId, Utility.ParseDInfo(company.TicketSysPassword, _appSettings.General.SysInfo));
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
                var caseInfo = JsonConvert.DeserializeObject<JIRACaseDetail>(await resp.Content.ReadAsStringAsync());

                caseDetail = new CaseDetail()
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
        /// Used to get the case status from JIRA
        /// </summary>
        /// <param name="company"></param>
        /// <param name="TicketSysCompanyCode"></param>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public async Task<CaseDetail> GetCaseStatusAsync(Company company, string TicketSysCompanyCode, string CaseId)
        {
            HttpResponseMessage resp = null;
            try
            {
                CaseDetail caseDetail = new CaseDetail();
                if (string.IsNullOrWhiteSpace(TicketSysCompanyCode))
                    throw new ArgumentException($"Please enter correct company code", "TicketSysCompanyCode");
                if (string.IsNullOrWhiteSpace(CaseId))
                    throw new ArgumentException($"Please enter correct CaseId", "CaseId");

                var JIRAReq = getJIRARequestObj(company.TicketSysId, Utility.ParseDInfo(company.TicketSysPassword, _appSettings.General.SysInfo));
                JIRAReq.Method = HttpMethod.Get;
                JIRAReq.RequestUri = new Uri(string.Format(_appSettings.JIRAApiEndpoint.GetStatus, company.TicketSysUrl, CaseId));

                resp = await RestApiHelper.SendAsync(JIRAReq);
                resp.EnsureSuccessStatusCode();
                var caseInfo = JsonConvert.DeserializeObject<JIRACaseDetail>(await resp.Content.ReadAsStringAsync());

                if (!caseInfo.serviceDeskId.ToString().Equals(TicketSysCompanyCode))
                    throw new ArgumentException($"This case ID {caseInfo.issueKey} doesnt link to your service desk. Please try again", "CaseId");

                caseDetail = new CaseDetail()
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

        /// <summary>
        /// Used to read the client companies like clientCompanyName
        /// </summary>
        /// <param name="company"></param>
        /// <param name="clientCompanyName"></param>
        /// <returns></returns>
        public async Task<List<ClientCompany>> GetClientCompanies(Company company, string clientCompanyName)
        {
            HttpResponseMessage resp = null;
            try
            {
                List<ClientCompany> companies = new List<ClientCompany>();

                if (string.IsNullOrWhiteSpace(clientCompanyName))
                    throw new ArgumentException($"Please enter correct cilent company name", "clientCompanyName");
                
                if (company == null)
                    throw new ArgumentException($"Please provide company data", "company");

                // get companies page by page to find the matching client company name.                
                int start = 0;
                int limit = 100;

                             

                //Looks like jira doesnt have an option to find the service desk using name. So we have to loop through all service desk and find matching names
                //This loop will find all matching records and breaks once it search through all companies.
                while (true)
                {
                    //New req need to create else client throws error [The request message was already sent.]
                    var JIRAReq = getJIRARequestObj(company.TicketSysId, Utility.ParseDInfo(company.TicketSysPassword, _appSettings.General.SysInfo));
                    JIRAReq.Method = HttpMethod.Get;
                    JIRAReq.RequestUri = new Uri(string.Format(_appSettings.JIRAApiEndpoint.GetServiceDesk, company.TicketSysUrl, start, limit));
                    resp = await RestApiHelper.SendAsync(JIRAReq);
                    resp.EnsureSuccessStatusCode();

                    //FilterCollection to get matching names
                    var servicedeskDetails = JsonConvert.DeserializeObject<ServicedeskDetails>(await resp.Content.ReadAsStringAsync());
                    if (servicedeskDetails.size > 0)
                    {
                        servicedeskDetails
                              .values
                              .Where(x => (
                                              (x?.projectName?.Contains(clientCompanyName, StringComparison.InvariantCultureIgnoreCase)) ?? false
                                          )
                                     )?.ToList()
                                     ?.ForEach(srvDsk =>
                                                  companies.Add(
                                                            new ClientCompany()
                                                            {
                                                                ClientCompanyName = srvDsk.projectName,
                                                                TicketSysCompanyCode = srvDsk.id,                                                                
                                                                Active = true,
                                                                CreatedOn = DateTime.Now,
                                                                VerificationEmail = company.contactEmail //currently using conpanys contact. user should call and get OTP from company
                                                            }                 
                                                            ));
                    }
                    else
                        break;
                    start += (limit + 1);                    
                }

                //need to get the email info of the customer
                //as there is no such api available, we are designing to send the OTP  to the Company email.

                return companies;
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
    }
}

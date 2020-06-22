
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketBOT.Core.Models;

namespace TicketBOT.Core.Services.Interfaces
{
    public interface ICaseMgmtService
    {
        Task<List<ClientCompany>> GetClientCompanies(Company company, string clientCompanyName);
        Task<CaseDetail> GetCaseStatusAsync(Company company, string TicketSysCompanyCode, string CaseId);
        Task<CaseDetail> CreateCaseAsync(Company company, ClientCompany clientCompany, string CaseSubject, string CaseDescription);
    }
}

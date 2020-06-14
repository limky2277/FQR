
using System.Threading.Tasks;
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface ICaseMgmtService
    {
        Task<CaseDetail> GetCaseStatusAsync(Company company, string TicketSysCompanyCode, string CaseId);
        Task<CaseDetail> CreateCaseAsync(Company company, string CaseSubject, string CaseDescription);
    }
}

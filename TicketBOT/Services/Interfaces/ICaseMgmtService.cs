
using TicketBOT.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface ICaseMgmtService
    {
        CaseDetails GetCaseStatus(string TicketSysCompanyCode, string CaseId);
        CaseDetails CreateCase(string CaseSubject, string CaseDescription);
    }
}

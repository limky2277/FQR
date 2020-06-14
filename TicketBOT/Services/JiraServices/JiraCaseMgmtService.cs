using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class JiraCaseMgmtService : ICaseMgmtService
    {
        public CaseDetails CreateCase(string CaseSubject, string CaseDescription)
        {
            throw new NotImplementedException();
        }

        public CaseDetails GetCaseStatus(string TicketSysCompanyCode, string CaseId)
        {
            throw new NotImplementedException();
        }
    }
}

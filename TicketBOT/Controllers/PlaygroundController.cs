using Microsoft.AspNetCore.Mvc;
using System;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;
namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaygroundController : ControllerBase
    {
        private readonly ICaseMgmtService _caseMgmtService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IFbApiClientService _fbApiClientService;
        private readonly CompanyService _companyService;

        public PlaygroundController(ICaseMgmtService caseMgmtService, IUserRegistrationService userRegistrationService, 
            IFbApiClientService fbApiClientService, CompanyService companyService)
        {
            _caseMgmtService = caseMgmtService;
            _userRegistrationService = userRegistrationService;
            _fbApiClientService = fbApiClientService;
            _companyService = companyService;
        }

        public IActionResult Get()
        {
            _companyService.Create(new Company { Id = Guid.NewGuid(), CompanyName = "ABC Company", FbPageId = "103650171367830", FbPageToken = "EAAJHOBuXMzYBAPpVpakroPUU8YE8w6ZC57iya27Dd769N4sWjIbvQSatPnIv4NCO4MWdUqbXiBjp5y4hNwqV2W3Jfi2XyqsylkSQqw5vbDipkEUZCPMnEvcUNXL3xM7dfW4DCLOHnBMEwOZC7vpWGcriKrLROVp7JNRhUarP84VlphtKO2Edar3BhuFopsZD" });

            return Ok(_companyService.Get());
        }
    }
}

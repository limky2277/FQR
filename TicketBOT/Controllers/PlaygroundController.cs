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
            return Ok(_companyService.Get());
        }
    }
}

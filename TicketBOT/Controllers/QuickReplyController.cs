using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickReplyController : ControllerBase
    {
        private ICaseMgmtService _caseMgmtService;
        private IUserRegistrationService _userRegistrationService;

        public QuickReplyController(ICaseMgmtService caseMgmtService, IUserRegistrationService userRegistrationService)
        {
            _caseMgmtService = caseMgmtService;
            _userRegistrationService = userRegistrationService;
        }

        [HttpGet]
        public string Get()
        {
            var resp = _userRegistrationService.GetUserInfo("1234");

            return resp;
        }
    }
}

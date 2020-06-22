using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        [HttpGet]
        public IActionResult Notify()
        {
            //loop through subscriptions

            //check jira status <> current status
            {
                //send notification to user 

                //IF only one time then delete entry

                
                //------------ ELSE
                //ask if user need to continue subscription

                //IF YES, UPDATE subscription entry with new token and status 
                //IF NO, delete entry

            }
        }
    }
}

using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;
using TicketBOT.Helpers;
using TicketBOT.Services.BotServices;

namespace TicketBOT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly OneTimeNotificationService _oneTimeNotifService;

        public NotificationController(OneTimeNotificationService oneTimeNotifService)
        {
            _oneTimeNotifService = oneTimeNotifService;
        }

        [HttpGet]
        public async Task<IActionResult> Notify()
        {
            try
            {
                await _oneTimeNotifService.BlastJiraStatusUpdateNotification();
                return Ok();
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, _logger, this.Request, this.RouteData);
                return Ok();
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketBOT.Core.Models;

namespace TicketBOT.Services.Interfaces
{
    public interface ITicketSysNotificationService : IGenericService<TicketSysNotification>
    {
        TicketSysNotification GetByUser(Guid id); 
    }
}

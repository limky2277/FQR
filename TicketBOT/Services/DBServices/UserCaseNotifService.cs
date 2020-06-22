using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.DBServices
{
    public class UserCaseNotifService : IGenericService<TicketSysNotification>
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<TicketSysNotification> _notif;

        public UserCaseNotifService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(DBHelper.getInfo(appSettings));
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _notif = database.GetCollection<TicketSysNotification>(nameof(TicketSysNotification));
        }

        public TicketSysNotification Create(TicketSysNotification ticketSysNotification)
        {
            // Duplicate check
            var validate = _notif.Find(x => x.JiraCaseKey == ticketSysNotification.JiraCaseKey && x.OneTimeNotifToken == ticketSysNotification.OneTimeNotifToken).ToList();
            if (validate.Count == 0)
            {
                _notif.InsertOne(ticketSysNotification);
                return ticketSysNotification;
            }
            return null;
        }

        public List<TicketSysNotification> Get() =>
            _notif.Find(x => true).ToList();

        public TicketSysNotification Get(string caseKey) =>
            _notif.Find(x => x.JiraCaseKey == caseKey).FirstOrDefault();

        public TicketSysNotification GetById(Guid id) =>
            _notif.Find(x => x.Id == id).FirstOrDefault();

        public void Update(Guid id, TicketSysNotification ticketSysNotification) =>
          _notif.ReplaceOne(x => x.Id == id, ticketSysNotification);

        public void Remove(TicketSysNotification ticketSysNotification) =>
            _notif.DeleteOne(x => x.Id == ticketSysNotification.Id);

        public void Remove(Guid id) =>
            _notif.DeleteOne(x => x.Id == id);
    }
}

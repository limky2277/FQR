using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class JiraUserMgmtService : IGenericService<JiraUser>, IUserMgmtService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<JiraUser> _user;

        public JiraUserMgmtService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _user = database.GetCollection<JiraUser>(nameof(JiraUser));
        }

        public List<JiraUser> Get() =>
            _user.Find(x => true).ToList();

        public JiraUser GetById(Guid id) =>
            _user.Find(x => x.Id == id).FirstOrDefault();

        public JiraUser Get(string userFbId) =>
           _user.Find(x => x.UserFbId == userFbId).FirstOrDefault();

        public JiraUser Create(JiraUser user)
        {
            // Duplicate check
            var validate = _user.Find<JiraUser>(x => x.UserFbId == user.UserFbId && x.CompanyId == user.CompanyId && x.Active == true).ToList();
            if (validate.Count == 0)
            {
                _user.InsertOne(user);
                return user;
            }
            return null;
        }

        public void Update(Guid id, JiraUser user) =>
           _user.ReplaceOne(x => x.Id == id, user);

        public void Remove(JiraUser user) =>
            _user.DeleteOne(x => x.Id == user.Id);

        public void Remove(Guid id) =>
            _user.DeleteOne(x => x.Id == id);

        public JiraUser GetUser(string userFbId, Guid companyId) =>
           _user.Find(x => x.UserFbId == userFbId && x.CompanyId == companyId && x.Active == true).FirstOrDefault();
    }
}

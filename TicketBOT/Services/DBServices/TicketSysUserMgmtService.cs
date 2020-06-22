using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TicketBOT.Core.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.DBServices
{
    public class TicketSysUserMgmtService : IGenericService<TicketSysUser>, IUserMgmtService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<TicketSysUser> _user;

        public TicketSysUserMgmtService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            //var client = new MongoClient("mongodb+srv://dbuser:<password>@cluster0-mbidz.mongodb.net/<dbname>?retryWrites=true&w=majority");
            //var database = client.GetDatabase("test");

            _user = database.GetCollection<TicketSysUser>(nameof(TicketSysUser));
        }

        public List<TicketSysUser> Get() =>
            _user.Find(x => true).ToList();

        public TicketSysUser GetById(Guid id) =>
            _user.Find(x => x.Id == id).FirstOrDefault();

        public TicketSysUser Get(string userFbId) =>
           _user.Find(x => x.UserFbId == userFbId).FirstOrDefault();

        public TicketSysUser Create(TicketSysUser user)
        {
            // Duplicate check
            var validate = _user.Find<TicketSysUser>(x => x.UserFbId == user.UserFbId && x.CompanyId == user.CompanyId && x.Active == true).ToList();
            if (validate.Count == 0)
            {
                _user.InsertOne(user);
                return user;
            }
            return null;
        }

        public void Update(Guid id, TicketSysUser user) =>
           _user.ReplaceOne(x => x.Id == id, user);

        public void Remove(TicketSysUser user) =>
            _user.DeleteOne(x => x.Id == user.Id);

        public void Remove(Guid id) =>
            _user.DeleteOne(x => x.Id == id);

        public TicketSysUser GetUser(string userFbId, Guid companyId) =>
           _user.Find(x => x.UserFbId == userFbId && x.CompanyId == companyId && x.Active == true).FirstOrDefault();
    }
}

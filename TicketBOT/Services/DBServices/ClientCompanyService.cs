using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using TicketBOT.Core.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.DBServices
{
    public class ClientCompanyService : IGenericService<ClientCompany>
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<ClientCompany> _client;

        public ClientCompanyService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _client = database.GetCollection<ClientCompany>(nameof(ClientCompany));
        }

        public List<ClientCompany> Get() =>
            _client.Find(x => true).ToList();

        public ClientCompany GetById(Guid id) =>
            _client.Find(x => x.Id == id).FirstOrDefault();

        public ClientCompany Get(string clientCompanyName) =>
            _client.Find(x => x.ClientCompanyName == clientCompanyName).FirstOrDefault();

        public ClientCompany Create(ClientCompany client)
        {
            // Duplicate check
            var validate = _client.Find(x => x.ClientCompanyName == client.ClientCompanyName).ToList();
            if (validate.Count == 0)
            {
                _client.InsertOne(client);
                return client;
            }
            return validate.FirstOrDefault();
        }

        public void Update(Guid id, ClientCompany company) =>
            _client.ReplaceOne(x => x.Id == id, company);

        public void Remove(ClientCompany company) =>
            _client.DeleteOne(x => x.Id == company.Id);

        public void Remove(Guid id) =>
            _client.DeleteOne(x => x.Id == id);
    }
}

using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class CompanyService : IGenericService<Company>
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<Company> _company;

        public CompanyService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _company = database.GetCollection<Company>(nameof(Company));
        }

        public List<Company> Get() =>
            _company.Find(x => true).ToList();

        public Company GetById(Guid id) =>
            _company.Find(x => x.Id == id).FirstOrDefault();

        public Company Get(string pageId) =>
            _company.Find(x => x.FbPageId == pageId).FirstOrDefault();

        public Company Create(Company company)
        {
            // Duplicate check
            var validate = _company.Find(x => x.FbPageId == company.FbPageId && x.FbPageToken == company.FbPageToken).ToList();
            if (validate.Count == 0)
            {
                _company.InsertOne(company);
                return company;
            }
            return null;
        }

        public void Update(Guid id, Company company) =>
            _company.ReplaceOne(x => x.Id == id, company);

        public void Remove(Company company) =>
            _company.DeleteOne(x => x.Id == company.Id);

        public void Remove(Guid id) =>
            _company.DeleteOne(x => x.Id == id);
    }
}

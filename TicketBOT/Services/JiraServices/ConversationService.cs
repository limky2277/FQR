using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class ConversationService : IGenericService<QAConversation>, IConversationService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<QAConversation> _conversation;

        public ConversationService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _conversation = database.GetCollection<QAConversation>(nameof(QAConversation));
        }

        public List<QAConversation> Get() =>
            _conversation.Find(x => true).ToList();

        public QAConversation GetById(Guid id) =>
            _conversation.Find<QAConversation>(x => x.Id == id).FirstOrDefault();

        public QAConversation Get(string id)
        {
            throw new NotImplementedException();
        }

        public QAConversation Create(QAConversation conversation)
        {
            _conversation.InsertOne(conversation);
            return conversation;
        }

        public void Update(Guid id, QAConversation conversation) =>
            _conversation.ReplaceOne(x => x.Id == id, conversation);

        public void Remove(QAConversation conversation) =>
            _conversation.DeleteOne(x => x.Id == conversation.Id);

        public void Remove(Guid id) =>
            _conversation.DeleteOne(x => x.Id == id);

        public QAConversation GetLastQuestion(Guid CompanyId, string FbSenderId) =>
            _conversation.Find<QAConversation>(x => x.Id == CompanyId).SortByDescending(x => x.ModifiedOn).FirstOrDefault();

    }
}

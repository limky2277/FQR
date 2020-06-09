using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class ConversationService : IGenericService<QnAConversation>, IConversationService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<QnAConversation> _conversation;

        public ConversationService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(_appSettings.TicketBOTDb.ConnectionString);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _conversation = database.GetCollection<QnAConversation>(nameof(QnAConversation));
        }

        public List<QnAConversation> Get() =>
            _conversation.Find(x => true).ToList();

        public QnAConversation GetById(Guid id) =>
            _conversation.Find<QnAConversation>(x => x.Id == id).FirstOrDefault();

        public QnAConversation Get(string id)
        {
            throw new NotImplementedException();
        }

        public QnAConversation Create(QnAConversation conversation)
        {
            _conversation.InsertOne(conversation);
            return conversation;
        }

        public void Update(Guid id, QnAConversation conversation) =>
            _conversation.ReplaceOne(x => x.Id == id, conversation);

        public void Remove(QnAConversation conversation) =>
            _conversation.DeleteOne(x => x.Id == conversation.Id);

        public void Remove(Guid id) =>
            _conversation.DeleteOne(x => x.Id == id);

        public QnAConversation GetLastQuestion(Guid CompanyId, string FbSenderId) =>
            _conversation.Find<QnAConversation>(x => x.Id == CompanyId).SortByDescending(x => x.ModifiedOn).FirstOrDefault();

    }
}

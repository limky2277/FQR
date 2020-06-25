using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.DBServices
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IMongoCollection<Conversation> _conversation;

        public ConversationService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            var client = DBHelper.getCient(appSettings);
            var database = client.GetDatabase(_appSettings.TicketBOTDb.DatabaseName);

            _conversation = database.GetCollection<Conversation>(nameof(Conversation));
        }

        //public bool AnyActiveConversation(string senderPageId, ConvLogType convLogType)
        //{
        //    var result = GetByKey(senderPageId);

        //    return result != null ? true : false;
        //}

        public ConversationData LastConversation(string senderPageId)
        {
            var result = GetConversationList(senderPageId);
            if (result != null)
            {
                return result.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
            }
            return null;
        }

        public void RemoveActiveConversation(string senderPageId)
        {
            var result = GetActiveConversation(senderPageId);
            _conversation.DeleteOne(x => x.Id == result.Id);
        }

        public void UpsertActiveConversation(string senderPageId, ConversationData conversations)
        {
            List<ConversationData> convList = GetConversationList(senderPageId);
            if (convList != null)
            {
                // If answer is satisfied, replace new
                if (conversations.Answered)
                {
                    var prevConv = convList.Find(x => x.LastQuestionAsked == conversations.LastQuestionAsked);
                    convList.Remove(prevConv);
                    convList.Add(conversations);
                }
                else
                {
                    var prevConv = convList.Find(x => x.LastQuestionAsked == conversations.LastQuestionAsked);
                    // New question by bot
                    if (prevConv == null)
                    {
                        convList.Add(conversations);
                    }
                }

                Conversation convUpd = GetActiveConversation(senderPageId);
                convUpd.ConversationData = JsonConvert.SerializeObject(convList);
                convUpd.ModifiedOn = DateTime.Now;

                Update(convUpd.Id, convUpd);
            }
            else
            {
                convList = new List<ConversationData>();
                convList.Add(conversations);

                Conversation newConv = new Conversation
                {
                    SenderPageId = senderPageId,
                    ConversationData = JsonConvert.SerializeObject(convList),
                    ModifiedOn = DateTime.Now
                };

                Create(newConv);
            }
        }

        public List<ConversationData> GetConversationList(string senderPageId)
        {
            var result = GetActiveConversation(senderPageId);
            if (result != null)
            {
                List<ConversationData> convs = JsonConvert.DeserializeObject<List<ConversationData>>(result.ConversationData).ToList();
                return convs;
            }
            return null;
        }

        public void Update(string id, Conversation conversation) =>
            _conversation.ReplaceOne(x => x.Id == id, conversation);

        public void Create(Conversation conversation)
        {
            // To-do: TTL (Time to Live)
            // Auto delete conversation data after x mins
            _conversation.InsertOne(conversation);
        }

        public Conversation GetActiveConversation(string senderPageId, ConvLogType convLogType = ConvLogType.ChatLog)
        {
            return _conversation.Find(x => x.SenderPageId == senderPageId
                                         && x.ModifiedOn > DateTime.UtcNow.AddMinutes(-_appSettings.ConversationSettings.ExpiryAfterMins)
                                         && x.ModifiedOn < DateTime.UtcNow
                                         && x.ConversationLogType == (int)convLogType)
                                         .FirstOrDefault();

        }
    }
}

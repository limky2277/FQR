
using EasyCaching.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TicketBOT.Models;
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.RedisServices
{
    public class SenderCacheService : ISenderCacheService
    {
        private readonly ApplicationSettings _appSettings;

        // Redis
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly IEasyCachingProviderFactory _cachingProviderFactory;

        public SenderCacheService(ApplicationSettings appSettings, IEasyCachingProviderFactory cachingProviderFactory)
        {
            _appSettings = appSettings;
            _cachingProviderFactory = cachingProviderFactory;
            _cachingProvider = _cachingProviderFactory.GetCachingProvider(_appSettings.RedisSettings.CachingProvider);
        }

        public bool AnyActiveConversation(string senderId)
        {
            var result = GetByKey(senderId);

            return result.HasValue;
        }

        public QAConversation LastConversation(string senderId)
        {
            var result = GetConversationList(senderId);
            if (result != null)
            {
                return result.OrderByDescending(x => x.ModifiedOn).FirstOrDefault();
            }
            return null;
        }

        public void RemoveActiveConversation(string senderId) =>
            _cachingProvider.Remove(senderId);

        public void UpsertActiveConversation(string senderId, QAConversation conversations)
        {
            List<QAConversation> convList = GetConversationList(senderId);
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
            }
            else
            {
                convList = new List<QAConversation>();
                convList.Add(conversations);
            }

            _cachingProvider.Set(senderId, JsonConvert.SerializeObject(convList), TimeSpan.FromMinutes(_appSettings.RedisSettings.TimeSpanMins));
        }

        private CacheValue<string> GetByKey(string senderId) =>
            _cachingProvider.Get<string>(senderId);

        public List<QAConversation> GetConversationList(string senderId)
        {
            var result = GetByKey(senderId);
            if (result.HasValue)
            {
                List<QAConversation> convs = JsonConvert.DeserializeObject<List<QAConversation>>(result.Value).ToList();
                return convs;
            }
            return null;
        }
    }
}

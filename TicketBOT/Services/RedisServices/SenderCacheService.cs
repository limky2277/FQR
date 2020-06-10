
using EasyCaching.Core;
using System;
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
            var result = _cachingProvider.Get<string>(senderId);

            return result.HasValue;
        }

        public void RemoveActiveConversation(string senderId) =>
            _cachingProvider.Remove(senderId);

        public void UpsertActiveConversation(string senderId, string value) =>
            _cachingProvider.Set(senderId, value, TimeSpan.FromMinutes(_appSettings.RedisSettings.TimeSpanMins));
    }
}

using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Bags_Shop_API.Services.Behaviors
{
    public class CachingBehavior<TRequest, TResponse>
      : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private readonly ICacheTokenProvider _cacheTokenProvider;

        public CachingBehavior(
            IMemoryCache cache,
            ILogger<CachingBehavior<TRequest, TResponse>> logger,
            ICacheTokenProvider cacheTokenProvider)
        {
            _cache = cache;
            _logger = logger;
            _cacheTokenProvider = cacheTokenProvider;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("inside pipeline");
            if (request is not ICacheableQuery cacheableQuery)
                return await next();

            var cacheKey = cacheableQuery.CacheKey;

            if (_cache.TryGetValue(cacheKey, out TResponse cachedData))
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                return cachedData;
            }

            var response = await next();

            if (response is Shared.IResult result && !result.Success)
            {
                 _logger.LogInformation(
                    "Skipping cache for key {CacheKey} because result failed",
                    cacheKey);
                return response;
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    cacheableQuery.CacheDuration ?? TimeSpan.FromMinutes(10),
                ExpirationTokens =
                {
                    new CancellationChangeToken(_cacheTokenProvider.Token)
                }
            };

            _cache.Set(cacheKey, response, cacheOptions);

            _logger.LogInformation("Added data for key {CacheKey} to cache", cacheKey);

            return response;
        }
    }
}

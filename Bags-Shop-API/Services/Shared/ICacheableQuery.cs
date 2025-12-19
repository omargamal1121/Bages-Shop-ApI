using System;

namespace Bags_Shop_API.Services.Shared
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan? CacheDuration { get; }
    }
}

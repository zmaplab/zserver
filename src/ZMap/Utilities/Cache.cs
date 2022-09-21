using System;
using Microsoft.Extensions.Caching.Memory;

namespace ZMap.Utilities;

public static class Cache
{
    private static readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

    public static TItem GetOrCreate<TItem>(string key, Func<ICacheEntry, TItem> valueFactory)
    {
        lock (typeof(Cache))
        {
            return MemoryCache.TryGetValue(key, out TItem item1) ? item1 : MemoryCache.GetOrCreate(key, valueFactory);
        }
    }
}
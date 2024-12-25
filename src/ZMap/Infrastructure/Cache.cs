namespace ZMap.Infrastructure;

public static class Cache
{
    private static readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

    public static TItem GetOrCreate<TItem>(string key, Func<ICacheEntry, TItem> valueFactory)
    {
        // comments: 应该是不需要 lock 的，但可能会产生多次创建
        // lock (typeof(Cache))
        // {
        return MemoryCache.TryGetValue(key, out TItem item1) ? item1 : MemoryCache.GetOrCreate(key, valueFactory);
        // }
    }

    public static async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<ICacheEntry, Task<TItem>> valueFactory)
    {
        // comments: 应该是不需要 lock 的，但可能会产生多次创建
        // lock (typeof(Cache))
        // {
        return MemoryCache.TryGetValue(key, out TItem item1)
            ? item1
            : await MemoryCache.GetOrCreateAsync(key, valueFactory);
        // }
    }
}
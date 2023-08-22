using System.Collections.Concurrent;
using Xunit;

namespace ZServer.Tests;

public class CacheTests
{
    [Fact]
    public void AddOrUpdate()
    {
        ConcurrentDictionary<string, int> cache = new();
        cache.AddOrUpdate("1", 1, (_, _) => 1);
        Assert.Equal(1, cache["1"]);
        cache.AddOrUpdate("1", 1, (_, _) => 2);
        Assert.Equal(2, cache["1"]);
        cache.AddOrUpdate("1", 3, (_, _) => 3);
        Assert.Equal(3, cache["1"]);
    }
}
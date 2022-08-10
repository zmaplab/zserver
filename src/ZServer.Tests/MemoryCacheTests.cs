using System;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ZServer.Tests
{
    public class MemoryCacheTests : BaseTests
    {
        [Fact]
        public void CreateOrGet()
        {
            var cache =  GetService<IMemoryCache>();
            var g1 = cache.GetOrCreate("test", _ => Guid.NewGuid());
            var g2 = cache.GetOrCreate("test", _ => Guid.NewGuid());
            Assert.Equal(g1, g2);
        }
    }
}
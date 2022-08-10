using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ZServer.Entity;

namespace ZServer.Store.Configuration
{
    public class ResourceGroupStore : IResourceGroupStore
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ServerOptions _options;

        public ResourceGroupStore(IConfiguration configuration,
            IOptionsMonitor<ServerOptions> options,
            IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
            _options = options.CurrentValue;
        }

        public async Task<ResourceGroup> FindAsync(string name)
        {
            return string.IsNullOrWhiteSpace(name)
                ? null
                : await _cache.GetOrCreateAsync($"{GetType().FullName}:{name}", entry =>
                {
                    var section = _configuration.GetSection($"resourceGroups:{name}");

                    var resourceGroup = section.Get<ResourceGroup>();
                    if (resourceGroup != null)
                    {
                        resourceGroup.Name = name;
                    }
                    
                    entry.SetValue(resourceGroup);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return Task.FromResult(resourceGroup);
                });
        }

        public async Task<List<ResourceGroup>> GetAllAsync()
        {
            var result = new List<ResourceGroup>();
            foreach (var child in _configuration.GetSection("resourceGroups").GetChildren())
            {
                result.Add(await FindAsync(child.Key));
            }

            return result;
        }
    }
}
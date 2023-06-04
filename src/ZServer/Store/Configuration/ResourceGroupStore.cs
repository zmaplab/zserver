using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZServer.Entity;

namespace ZServer.Store.Configuration
{
    public class ResourceGroupStore : IResourceGroupStore
    {
        private static Dictionary<string, ResourceGroup> _cache = new();

        public Task Refresh(IConfiguration configuration)
        {
            var sections = configuration.GetSection("resourceGroups");
            var dict = new Dictionary<string, ResourceGroup>();
            foreach (var section in sections.GetChildren())
            {
                var resourceGroup = section.Get<ResourceGroup>();
                if (resourceGroup == null)
                {
                    continue;
                }

                resourceGroup.Name = section.Key;
                dict.TryAdd(section.Key, resourceGroup);
            }

            _cache = dict;
            return Task.CompletedTask;
        }

        public async Task<ResourceGroup> FindAsync(string name)
        {
            if (_cache.TryGetValue(name, out var resourceGroup))
            {
                return await Task.FromResult(resourceGroup.Clone());
            }

            return null;
        }

        public Task<List<ResourceGroup>> GetAllAsync()
        {
            var items = _cache.Values.Select(x => x.Clone()).ToList();
            return Task.FromResult(items);
        }
    }
}
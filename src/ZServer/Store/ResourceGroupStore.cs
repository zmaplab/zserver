using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZMap;

namespace ZServer.Store;

public class ResourceGroupStore : IResourceGroupStore
{
    private static Dictionary<string, ResourceGroup> _cache = new();

    public ValueTask<ResourceGroup> FindAsync(string name)
    {
        return _cache.TryGetValue(name, out var resourceGroup)
            ? new ValueTask<ResourceGroup>(resourceGroup.Clone())
            : new ValueTask<ResourceGroup>();
    }

    public ValueTask<List<ResourceGroup>> GetAllAsync()
    {
        var items = _cache.Values.Select(x => x.Clone()).ToList();
        return new ValueTask<List<ResourceGroup>>(items);
    }

    public Task Refresh(List<JObject> configurations)
    {
        var dict = new Dictionary<string, ResourceGroup>();

        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("resourceGroups");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var name = section.Name;
                var entity = section.Value.ToObject<ResourceGroup>();
                entity.Name = name;

                dict.TryAdd(name, entity);
            }
        }

        _cache = dict;
        return Task.CompletedTask;
    }

    // public Task Refresh(IEnumerable<IConfiguration> configurations)
    // {
    //     var dict = new Dictionary<string, ResourceGroup>();
    //
    //     foreach (var configuration in configurations)
    //     {
    //         var sections = configuration.GetSection("resourceGroups");
    //         foreach (var section in sections.GetChildren())
    //         {
    //             var resourceGroup = section.Get<ResourceGroup>();
    //             if (resourceGroup == null)
    //             {
    //                 continue;
    //             }
    //
    //             resourceGroup.Name = section.Key;
    //             dict.TryAdd(section.Key, resourceGroup);
    //         }
    //     }
    //
    //     _cache = dict;
    //     return Task.CompletedTask;
    // }
}
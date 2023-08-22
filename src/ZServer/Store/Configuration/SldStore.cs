using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZMap.SLD;
using ZMap.Style;

namespace ZServer.Store.Configuration;

public class SldStore : ISldStore
{
    private static readonly ConcurrentDictionary<string, List<StyleGroup>> Cache = new();

    public Task<List<StyleGroup>> FindAsync(string name)
    {
        var result = Cache.TryGetValue(name, out var value) ? value : null;
        return Task.FromResult(result);
    }

    public Task Refresh(IConfiguration configuration)
    {
        var dir = "sld";
        if (!Directory.Exists(dir))
        {
            return Task.CompletedTask;
        }

        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            var sld = StyledLayerDescriptor.Load(File.OpenRead(file));
            var visitor = new SldStyleVisitor();
            sld.Accept(visitor, null);

            if (!string.IsNullOrWhiteSpace(sld.Name))
            {
                Cache.AddOrUpdate(sld.Name, visitor.StyleGroups, (_, _) => visitor.StyleGroups);
            }
        }

        return Task.CompletedTask;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZMap.SLD;
using ZMap.Style;

namespace ZServer.Store.Configuration;

public class SldStore : ISldStore
{
    private static readonly Dictionary<string, List<StyleGroup>> Cache;

    static SldStore()
    {
        Cache = new Dictionary<string, List<StyleGroup>>();
        var dir = "Sld";
        if (!Directory.Exists(dir))
        {
            return;
        }

        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            var sld = StyledLayerDescriptor.Load(File.OpenRead(file));
            var visitor = new SldStyleVisitor();
            sld.Accept(visitor, null);

            if (!string.IsNullOrWhiteSpace(sld.Name) && !Cache.ContainsKey(sld.Name))
            {
                Cache.Add(sld.Name, visitor.StyleGroups);
            }
        }
    }

    public Task<List<StyleGroup>> FindAsync(string name)
    {
        var result = Cache.ContainsKey(name) ? Cache[name] : null;
        return Task.FromResult(result);
    }
}
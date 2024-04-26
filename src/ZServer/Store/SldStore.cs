using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ZMap.SLD;
using ZMap.Style;

namespace ZServer.Store;

public class SldStore : ISldStore
{
    private static readonly ConcurrentDictionary<string, List<StyleGroup>> Cache = new();

    public Task<List<StyleGroup>> FindAsync(string name)
    {
        var result = Cache.GetValueOrDefault(name);
        return Task.FromResult(result);
    }

    public Task Refresh(List<JObject> configurations)
    {
        var dir = "sld";
        if (!Directory.Exists(dir))
        {
            return Task.CompletedTask;
        }

        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();

        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            var sld = StyledLayerDescriptor.Load(File.OpenRead(file));
            var visitor = new SldStyleVisitor();
            sld.Accept(visitor, null);

            if (string.IsNullOrWhiteSpace(sld.Name))
            {
                continue;
            }

            keys.Add(sld.Name);
            Cache.AddOrUpdate(sld.Name, visitor.StyleGroups, (_, _) => visitor.StyleGroups);
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }

        return Task.CompletedTask;
    }

    public Task Refresh(IEnumerable<IConfiguration> __)
    {
        var dir = "sld";
        if (!Directory.Exists(dir))
        {
            return Task.CompletedTask;
        }

        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();

        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            var sld = StyledLayerDescriptor.Load(File.OpenRead(file));
            var visitor = new SldStyleVisitor();
            sld.Accept(visitor, null);

            if (string.IsNullOrWhiteSpace(sld.Name))
            {
                continue;
            }

            keys.Add(sld.Name);
            Cache.AddOrUpdate(sld.Name, visitor.StyleGroups, (_, _) => visitor.StyleGroups);
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }

        return Task.CompletedTask;
    }
}
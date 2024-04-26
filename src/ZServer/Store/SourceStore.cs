using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;
using ZMap.Source;

namespace ZServer.Store;

/// <summary>
/// 
/// </summary>
public class SourceStore : ISourceStore
{
    private static readonly ILogger Logger = Log.CreateLogger<SourceStore>();
    private static readonly ConcurrentDictionary<string, ISource> Cache = new();

    private static readonly ConcurrentDictionary<Type, ParameterInfo[]>
        StorageTypeCache =
            new();

    public Task Refresh(List<JObject> configurations)
    {
        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();

        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("sources");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var source = Get(section.Name, section.Value);
                if (source == null)
                {
                    continue;
                }

                source.Name = section.Name;
                keys.Add(source.Name);
                Cache.AddOrUpdate(source.Name, source, (_, _) => source);
            }
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }

        return Task.CompletedTask;
    }

    public Task Refresh(IEnumerable<IConfiguration> configurations)
    {
        var existKeys = Cache.Keys.ToList();
        var keys = new List<string>();

        foreach (var configuration in configurations)
        {
            var sections = configuration.GetSection("sources");
            foreach (var section in sections.GetChildren())
            {
                var source = Get(section);
                if (source == null)
                {
                    continue;
                }

                source.Name = section.Key;
                keys.Add(source.Name);
                Cache.AddOrUpdate(source.Name, source, (_, _) => source);
            }
        }

        var removedKeys = existKeys.Except(keys);
        foreach (var removedKey in removedKeys)
        {
            Cache.TryRemove(removedKey, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 不需要缓存，Source 直接存于 Layer 中，Layer 会缓存
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<ISource> FindAsync(string name)
    {
        if (Cache.TryGetValue(name, out var source))
        {
            return await Task.FromResult(source.Clone());
        }

        return null;
        // comments: 必须复制对像，不然并发情况会异常
    }

    public Task<List<ISource>> GetAllAsync()
    {
        var items = Cache.Values.Select(x => x.Clone()).ToList();
        return Task.FromResult(items);
    }

    private ISource Get(IConfigurationSection section)
    {
        var provider = section.GetSection("provider").Get<string>();
        if (string.IsNullOrWhiteSpace(provider))
        {
            Logger.LogError("数据源 {Name} 不存在或驱动为空", section.Key);
            return null;
        }

        var type = Type.GetType(provider);
        if (type == null)
        {
            Logger.LogError("数据源 {Name} 的驱动 {Provider} 不存在", section.Key, provider);
            return null;
        }

        var parameterInfos = StorageTypeCache.GetOrAdd(type, x =>
        {
            if (!typeof(IVectorSource).IsAssignableFrom(x))
            {
                Logger.LogError("数据源 {Name} 的驱动 {Provider} 不是有效的驱动", section.Key, provider);
                return null;
            }

            var result = x.GetConstructors()[0].GetParameters();

            return result;
        });

        ISource source;

        if (parameterInfos.Length == 0)
        {
            source = (ISource)Activator.CreateInstance(type);
        }
        else
        {
            var args = new object[parameterInfos.Length];
            for (var i = 0; i < args.Length; ++i)
            {
                var name = parameterInfos[i].Name;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                args[i] = section.GetSection(name).Get(parameterInfos[i].ParameterType);
            }

            try
            {
                source = (ISource)Activator.CreateInstance(type, args);
            }
            catch (Exception e)
            {
                Logger.LogError("创建数据源 {Name} 的驱动 {Provider} 失败: {Exception}", section.Key, provider,
                    e.ToString());
                return null;
            }
        }

        return source;
    }

    private ISource Get(string sourceName, JToken section)
    {
        var provider = section.Value<string>("provider");
        if (string.IsNullOrWhiteSpace(provider))
        {
            Logger.LogError("数据源 {Name} 不存在或驱动为空", sourceName);
            return null;
        }

        var type = Type.GetType(provider);
        if (type == null)
        {
            Logger.LogError("数据源 {Name} 的驱动 {Provider} 不存在", sourceName, provider);
            return null;
        }

        var parameterInfos = StorageTypeCache.GetOrAdd(type, x =>
        {
            if (!typeof(IVectorSource).IsAssignableFrom(x))
            {
                Logger.LogError("数据源 {Name} 的驱动 {Provider} 不是有效的驱动", sourceName, provider);
                return null;
            }

            var result = x.GetConstructors()[0].GetParameters();

            return result;
        });

        ISource source;

        if (parameterInfos.Length == 0)
        {
            source = (ISource)Activator.CreateInstance(type);
        }
        else
        {
            var args = new object[parameterInfos.Length];
            for (var i = 0; i < args.Length; ++i)
            {
                var name = parameterInfos[i].Name;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                args[i] = section[name].ToObject(parameterInfos[i].ParameterType);
            }

            try
            {
                source = (ISource)Activator.CreateInstance(type, args);
            }
            catch (Exception e)
            {
                Logger.LogError("创建数据源 {Name} 的驱动 {Provider} 失败: {Exception}", sourceName, provider,
                    e.ToString());
                return null;
            }
        }

        return source;
    }
}
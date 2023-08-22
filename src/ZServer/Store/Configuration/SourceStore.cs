using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZMap.Source;

namespace ZServer.Store.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class SourceStore : ISourceStore
    {
        private readonly ILogger<SourceStore> _logger;
        private static readonly ConcurrentDictionary<string, ISource> Cache = new();

        private static readonly ConcurrentDictionary<Type, ParameterInfo[]>
            StorageTypeCache =
                new();

        public SourceStore(
            ILogger<SourceStore> logger)
        {
            _logger = logger;
        }

        public Task Refresh(IConfiguration configuration)
        {
            var sections = configuration.GetSection("sources");
            foreach (var section in sections.GetChildren())
            {
                var source = Get(section);
                if (source != null)
                {
                    source.Name = section.Key;
                    Cache.AddOrUpdate(section.Key, source, (_, _) => source);
                }
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
                _logger.LogError("数据源 {Name} 不存在或驱动为空", section.Key);
                return null;
            }

            var type = Type.GetType(provider);
            if (type == null)
            {
                _logger.LogError("数据源 {Name} 的驱动 {Provider} 不存在", section.Key, provider);
                return null;
            }

            var parameterInfos = StorageTypeCache.GetOrAdd(type, x =>
            {
                if (!typeof(IVectorSource).IsAssignableFrom(x))
                {
                    _logger.LogError("数据源 {Name} 的驱动 {Provider} 不是有效的驱动", section.Key, provider);
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
                    args[i] = section.GetSection(parameterInfos[i].Name).Get(parameterInfos[i].ParameterType);
                }

                source = (ISource)Activator.CreateInstance(type, args);
            }

            return source;
        }
    }
}
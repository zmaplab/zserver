using System.Collections.Generic;

namespace ZMap.Extensions
{
    public static class DictionaryExtensions
    {
        public static object GetOrDefault(this IDictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var result) ? result : null;
        }
    }
}
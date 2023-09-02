using System;
using System.Collections.Generic;

namespace ZMap.Extensions
{
    public static class DictionaryExtensions
    {
        public static object GetOrDefault(this IDictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var result) ? result : null;
        }
        
        public static string GetTraceIdentifier(this IDictionary<string, object> dict)
        {
            return dict.TryGetValue(Defaults.TraceIdentifier, out var result)
                ? result.ToString()
                : Guid.NewGuid().ToString("N");
        }
    }
}
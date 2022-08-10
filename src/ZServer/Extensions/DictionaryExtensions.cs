using System;
using System.Collections.Generic;

namespace ZServer.Extensions
{
    public static class DictionaryExtensions
    {
        public static string GetTraceIdentifier(this IDictionary<string, object> dict)
        {
            return dict.TryGetValue(Constants.TraceIdentifier, out var result)
                ? result.ToString()
                : Guid.NewGuid().ToString("N");
        }
    }
}
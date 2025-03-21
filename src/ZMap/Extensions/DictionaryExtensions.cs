using MongoDB.Bson;

namespace ZMap.Extensions;

public static class DictionaryExtensions
{
    public static string GetTraceIdentifier(this IDictionary<string, object> dict)
    {
        return dict.TryGetValue(Defaults.TraceIdentifier, out var result)
            ? result.ToString()
            : ObjectId.GenerateNewId().ToString();
    }
}
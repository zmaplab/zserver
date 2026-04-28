using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZMap;

public static class FilterMerger
{
    public static string Merge(string defaultFilter, string requestFilter)
    {
        var hasDefault = !string.IsNullOrWhiteSpace(defaultFilter);
        var hasRequest = !string.IsNullOrWhiteSpace(requestFilter);

        if (!hasDefault && !hasRequest) return null;
        if (!hasDefault) return requestFilter;
        if (!hasRequest) return defaultFilter;

        var defaultInfo = JObject.Parse(defaultFilter);
        var defaultFilters = defaultInfo.Property("Filters")?.Value as JArray ?? new JArray();

        var requestInfo = JObject.Parse(requestFilter);
        var requestFilters = requestInfo.Property("Filters")?.Value as JArray ?? new JArray();

        if (defaultFilters.Count == 0 && requestFilters.Count == 0)
        {
            return null;
        }

        var requestFields = new HashSet<string>();
        foreach (var rf in requestFilters)
        {
            var field = rf["Field"]?.ToString();
            if (field != null) requestFields.Add(field);
        }

        var merged = new JArray();
        foreach (var df in defaultFilters)
        {
            var field = df["Field"]?.ToString();
            if (field != null && requestFields.Contains(field)) continue;
            merged.Add(df);
        }

        foreach (var rf in requestFilters)
        {
            merged.Add(rf);
        }

        if (merged.Count == 0)
        {
            return null;
        }

        var result = new JObject
        {
            ["Logic"] = "And",
            ["Filters"] = merged
        };

        return result.ToString(Newtonsoft.Json.Formatting.None);
    }
}

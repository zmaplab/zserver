using System.Text.Json;
using NetTopologySuite.IO.Converters;

namespace ZServer.Grains;

public static class GeoJsonSerializer
{
    private static readonly JsonSerializerOptions Options;

    static GeoJsonSerializer()
    {
        Options = new JsonSerializerOptions();
        Options.Converters.Add(new GeoJsonConverterFactory());
    }

    public static string Serialize(object a)
    {
        return JsonSerializer.Serialize(a, Options);
    }
}
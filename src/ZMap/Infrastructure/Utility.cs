namespace ZMap.Infrastructure;

/// <summary>
/// 常用的方法
/// </summary>
public static partial class Utility
{
    public static void PrintInfo()
    {
        var info = """
                    ______ _____
                   |___  // ____|
                      / /| (___   ___ _ ____   _____ _ __
                     / /  \___ \ / _ \ '__\ \ / / _ \ '__|
                    / /__ ____) |  __/ |   \ V /  __/ |
                   /_____|_____/ \___|_|    \_/ \___|_|
                   :: zserver ::             (v0.9.0.beta)

                   """;
        Console.WriteLine(info);
    }

    /// <summary>
    /// WMTS 缓存文件的 Key
    /// </summary>
    /// <param name="layers"></param>
    /// <param name="filter"></param>
    /// <param name="format"></param>
    /// <param name="tileMatrixSet"></param>
    /// <param name="tileMatrix"></param>
    /// <param name="tileRow"></param>
    /// <param name="tileCol"></param>
    /// <returns></returns>
    public static (string FullPath, string IntervalPath) GetWmtsPath(string layers, string filter, string format,
        string tileMatrixSet,
        string tileMatrix, int tileRow,
        int tileCol)
    {
        // group1:layer1,layer2 -> group1_layer1.group2_layer2
        var layerKey = layers.Replace(',', '.').Replace(':', '_');
        var cqlFilterKey = string.IsNullOrWhiteSpace(filter)
            ? string.Empty
            : CryptographyUtility.ComputeHash(Encoding.UTF8.GetBytes(filter));

        var imageExtension = GetImageExtension(format);

        var tileMatrixSetPath = tileMatrixSet.Replace(":", "").Replace("/", "_");
        var intervalPath = string.IsNullOrEmpty(cqlFilterKey)
            ? $"wmts{Path.DirectorySeparatorChar}{layerKey}{Path.DirectorySeparatorChar}{tileMatrixSetPath}{Path.DirectorySeparatorChar}{tileMatrix}{Path.DirectorySeparatorChar}{tileRow}{Path.DirectorySeparatorChar}{tileCol}{imageExtension}"
            : $"wmts{Path.DirectorySeparatorChar}{layerKey}{Path.DirectorySeparatorChar}{tileMatrixSetPath}{Path.DirectorySeparatorChar}{tileMatrix}{Path.DirectorySeparatorChar}{tileRow}{Path.DirectorySeparatorChar}{tileCol}_{cqlFilterKey}{imageExtension}";
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", intervalPath);
        return (fullPath, intervalPath);
    }

    // public static string GetWmtsKey(string layers, string tileMatrix, int tileRow,
    //     int tileCol)
    // {
    //     var layerKey = layers.Replace(',', '_');
    //     return $"{layerKey}/{tileMatrix}/{tileRow}/{tileCol}";
    // }

    public static int GetDpi(string formatOptions)
    {
        var dpi = 96;
        if (string.IsNullOrWhiteSpace(formatOptions))
        {
            return dpi;
        }

        var match = DpiRegex().Match(formatOptions);
        if (!match.Success)
        {
            return dpi;
        }

        var value = match.Value.Substring(4, match.Value.Length - 4);
        return int.TryParse(value, out dpi) ? dpi : 96;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetImageExtension(string format)
    {
        return format switch
        {
            "image/bmp" => ".bmp",
            "image/gif" => ".gif",
            "image/ico" => ".ico",
            "image/jpeg" => ".jpeg",
            "image/png" => ".png",
            "image/wbmp" => ".wbmp",
            "image/webp" => ".webp",
            "image/pkm" => ".pkm",
            "image/ktx" => ".ktx",
            "image/astc" => ".astc",
            "image/dng" => ".dng",
            "image/heif" => ".heif",
            "image/avif" => ".avif",
            _ => ".png"
        };
    }

    [GeneratedRegex("dpi:\\d+")]
    private static partial Regex DpiRegex();
}
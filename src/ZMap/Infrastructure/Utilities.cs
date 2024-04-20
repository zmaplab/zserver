using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Int32;

namespace ZMap.Infrastructure;

/// <summary>
/// 常用的方法
/// </summary>
public static class Utilities
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
    /// <param name="cqlFilter"></param>
    /// <param name="format"></param>
    /// <param name="tileMatrix"></param>
    /// <param name="tileRow"></param>
    /// <param name="tileCol"></param>
    /// <returns></returns>
    public static string GetWmtsPath(string layers, string cqlFilter, string format, string tileMatrix, int tileRow,
        int tileCol)
    {
        var layerKey = layers.Replace(',', '_');
        var cqlFilterKey = string.IsNullOrWhiteSpace(cqlFilter)
            ? string.Empty
            : MurmurHashAlgorithmUtilities.ComputeHash(Encoding.UTF8.GetBytes(cqlFilter));

        var imageExtension = GetImageExtension(format);
        return Path.Combine(AppContext.BaseDirectory, "cache", "wmts",
            string.IsNullOrEmpty(cqlFilterKey)
                ? $"{layerKey}/{tileMatrix}/{tileRow}/{tileCol}{imageExtension}"
                : $"{layerKey}/{tileMatrix}/{tileRow}/{tileCol}_{cqlFilterKey}{imageExtension}");
    }

    public static string GetWmtsKey(string layers, string tileMatrix, int tileRow,
        int tileCol)
    {
        var layerKey = layers.Replace(',', '_');
        return $"{layerKey}/{tileMatrix}/{tileRow}/{tileCol}";
    }

    public static int GetDpi(string formatOptions)
    {
        var dpi = 96;
        if (string.IsNullOrWhiteSpace(formatOptions))
        {
            return dpi;
        }

        var match = Regex.Match(formatOptions, "dpi:\\d+");
        if (!match.Success)
        {
            return dpi;
        }

        var value = match.Value.Substring(4, match.Value.Length - 4);
        TryParse(value, out dpi);

        return dpi;
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
}
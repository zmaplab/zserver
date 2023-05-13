using System.Text.RegularExpressions;

namespace ZMap.Infrastructure;

/// <summary>
/// 常用的方法
/// </summary>
public static class Utilities
{
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
        int.TryParse(value, out dpi);

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
using System.Text.RegularExpressions;

namespace ZMap.Infrastructure;

public static class CommonUtilities
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
    /// TODO: more extension names
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetImageExtension(string format)
    {
        return format switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpeg",
            _ => ".png"
        };
    }
}
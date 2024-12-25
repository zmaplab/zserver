namespace ZMap.Extensions;

public static class UriExtensions
{
    public static string GetPath(this Uri uri)
    {
        var schemaLength = uri.Scheme.Length + 3;
        return uri.OriginalString.Substring(schemaLength, uri.OriginalString.Length - schemaLength);
    }
}
using System;

namespace ZMap.Renderer.SkiaSharp.Extensions
{
    public static class UriExtensions
    {
        public static string ToPath(this Uri uri)
        {
            var schemaLenght = uri.Scheme.Length + 3;
            return uri.OriginalString.Substring(schemaLenght, uri.OriginalString.Length - schemaLenght);
        }
    }
}
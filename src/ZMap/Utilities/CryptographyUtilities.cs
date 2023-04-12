using System;
using System.Security.Cryptography;
using System.Text;

namespace ZMap.Utilities;

public class CryptographyUtilities
{
    public static string ComputeMD5(byte[] bytes)
    {
        var hash = MD5.Create();
        bytes = hash.ComputeHash(bytes);
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append($"{b:x2}");
        }

        return builder.ToString();
    }

    public static string ComputeMD5(string value, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new NullReferenceException("Compute MD5 value is null");
        }

        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        return ComputeMD5(encoding.GetBytes(value));
    }
}
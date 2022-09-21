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
}
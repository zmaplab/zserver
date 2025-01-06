using System.Security.Cryptography;

namespace ZMap.Infrastructure;

public static class CryptographyUtility
{
    public static string ComputeHash(byte[] bytes)
    {
        var hashBytes = MD5.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
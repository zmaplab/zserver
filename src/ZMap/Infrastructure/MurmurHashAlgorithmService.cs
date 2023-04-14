using System.Security.Cryptography;
using System.Text;
using Murmur;

namespace ZMap.Infrastructure;

public static class MurmurHashAlgorithmService
{
    public static string ComputeHash(byte[] bytes)
    {
        var murmur128 = MurmurHash.Create128();
        //申请获取锁
        var hashBytes = murmur128.ComputeHash(bytes);
        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append($"{b:x2}");
        }

        return builder.ToString();
    }
}
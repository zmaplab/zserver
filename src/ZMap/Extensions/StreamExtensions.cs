using System;
using System.IO;
using System.Threading.Tasks;

namespace ZMap.Extensions;

public static class StreamExtensions
{
    /// <summary>
    /// 把 Stream 转换成 byte[]
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<byte[]> ToArrayAsync(this Stream stream)
    {
        if (stream is MemoryStream ms)
        {
            return ms.ToArray();
        }

        if (stream.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var bytes = new byte[stream.Length];
        _ = await stream.ReadAsync(bytes, 0, bytes.Length);
        return bytes;
    }
}
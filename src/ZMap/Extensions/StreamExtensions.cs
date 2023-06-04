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
        var bytes = new byte[stream.Length];
        var _ = await stream.ReadAsync(bytes, 0, bytes.Length);
        return bytes;
    }
}
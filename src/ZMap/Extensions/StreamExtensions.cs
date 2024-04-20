using System.IO;

namespace ZMap.Extensions;

public static class StreamExtensions
{
    /// <summary>
    /// 把 Stream 转换成 byte[]
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ToArray(this Stream stream)
    {
        // TODO:  benchmark this
        var reader = new BinaryReader(stream);
        return reader.ReadBytes((int)stream.Length);
        // var bytes = new byte[stream.Length];
        // _ = await stream.ReadAsync(bytes, 0, bytes.Length);
        // return bytes;
    }
}
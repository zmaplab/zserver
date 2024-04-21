using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ZMap.Extensions;

namespace ZServer.Benchmark;

public class StreamToArray
{
    private static readonly byte[] Bytes = File.ReadAllBytes("xiongmao.jpg");

    [Benchmark]
    public async Task StreamToArrayTest()
    {
        using var stream = new MemoryStream(Bytes);
        var result = await stream.ToArrayAsync();
    }
}
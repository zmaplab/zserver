using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class MurmurHashAlgorithmServiceTests
{
    [Fact]
    public void Hash1()
    {
        Parallel.For(0, 1000, i =>
        {
            var key = Guid.NewGuid().ToString();
            MurmurHashAlgorithmService.ComputeHash(Encoding.UTF8.GetBytes(key));
        });
    }
}
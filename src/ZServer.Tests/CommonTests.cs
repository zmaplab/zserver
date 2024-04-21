using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class CommonTests
{
    [Fact]
    public void GetDpi()
    {
        var dpi1 = Utility.GetDpi("dpi:180");
        Assert.Equal(180, dpi1);

        var dpi2 = Utility.GetDpi("dpi:90");
        Assert.Equal(90, dpi2);
    }

    [Fact]
    public void Convert()
    {
        var result = ZMap.Infrastructure.Convert.ToObject<int?>("2");
        Assert.Equal(2, result);
    }
}
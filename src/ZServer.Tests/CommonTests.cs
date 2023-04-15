using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class CommonTests
{
    [Fact]
    public void GetDpi()
    {
        var dpi1 = Utilities.GetDpi("dpi:180");
        Assert.Equal(180, dpi1);

        var dpi2 = Utilities.GetDpi("dpi:90");
        Assert.Equal(90, dpi2);
    }
}
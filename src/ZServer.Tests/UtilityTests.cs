using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class UtilityTests
{
    [Fact]
    public void GetWmtsPath()
    {
        var b = Utility.GetWmtsPath("abcd", "Equal", "image/png", "ESPG:4326", "14", 1, 1);
        Assert.Equal("wmts/abcd/ESPG4326/14/1/1_F5F286E73BDA105E538310B3190F75C5.png", b.IntervalPath);
    }
}
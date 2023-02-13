using Xunit;
using ZMap.SLD.Filter;

namespace ZServer.Tests;

public class MethodsTests
{
 

    [Fact]
    public void IntTest()
    {
        var s1 = "1";
        var s2 = "1";
        var a = s1.CompareTo(s2);
        Assert.True(Methods.EqualTo(1, "1"));
        Assert.True(Methods.EqualTo("1", 1));

        Assert.True(Methods.EqualTo(1m, "1"));
        Assert.True(Methods.EqualTo("1", 1m));

        Assert.True(Methods.EqualTo(1f, "1"));
        Assert.True(Methods.EqualTo("1", 1f));

        Assert.True(Methods.EqualTo(1d, "1"));
        Assert.True(Methods.EqualTo("1", 1d));

        ////
        Assert.False(Methods.EqualTo(1, "1.0"));
        Assert.False(Methods.EqualTo("1.0", 1));

        Assert.True(Methods.EqualTo(1m, "1.0"));
        Assert.True(Methods.EqualTo("1.0", 1m));

        Assert.True(Methods.EqualTo(1f, "1.0"));
        Assert.True(Methods.EqualTo("1.0", 1f));

        Assert.True(Methods.EqualTo(1d, "1.0"));
        Assert.True(Methods.EqualTo("1.0", 1d));

        Assert.True(Methods.EqualTo(1, 1.0f));
        Assert.True(Methods.EqualTo(1, 1.2f));
    }
}
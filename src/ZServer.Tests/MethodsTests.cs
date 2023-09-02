using Xunit;
using ZMap;

namespace ZServer.Tests;

public class FunctionsTests
{
 

    [Fact(Skip = "SLD 实现验证")]
    public void IntTest()
    {
        var s1 = "1";
        var s2 = "1";
        var a = s1.CompareTo(s2);
        Assert.True(Functions.EqualTo(1, "1"));
        Assert.True(Functions.EqualTo("1", 1));

        Assert.True(Functions.EqualTo(1m, "1"));
        Assert.True(Functions.EqualTo("1", 1m));

        Assert.True(Functions.EqualTo(1f, "1"));
        Assert.True(Functions.EqualTo("1", 1f));

        Assert.True(Functions.EqualTo(1d, "1"));
        Assert.True(Functions.EqualTo("1", 1d));

        ////
        Assert.False(Functions.EqualTo(1, "1.0"));
        Assert.False(Functions.EqualTo("1.0", 1));

        Assert.True(Functions.EqualTo(1m, "1.0"));
        Assert.True(Functions.EqualTo("1.0", 1m));

        Assert.True(Functions.EqualTo(1f, "1.0"));
        Assert.True(Functions.EqualTo("1.0", 1f));

        Assert.True(Functions.EqualTo(1d, "1.0"));
        Assert.True(Functions.EqualTo("1.0", 1d));

        Assert.True(Functions.EqualTo(1, 1.0f));
        Assert.True(Functions.EqualTo(1, 1.2f));
    }
}
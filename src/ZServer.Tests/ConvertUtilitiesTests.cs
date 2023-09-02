using System;
using System.Text.Json;
using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class ConvertUtilitiesTests
{
    [Fact]
    public void String()
    {
        var result = ConvertUtilities.ToArray<string>("1 2 3");
        Assert.Equal(3, result.Length);
        Assert.Equal("1", result[0]);
        Assert.Equal("2", result[1]);
        Assert.Equal("3", result[2]);
    }

    [Fact]
    public void Int()
    {
        var result = ConvertUtilities.ToArray<int>("1 2 3");
        Assert.Equal(3, result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
    }
}
using System;
using System.IO;
using Xunit;
using ZMap.Extensions;

namespace ZServer.Tests;

public class UriTests
{
    [Fact]
    public void GetPath()
    {
        var uri1 = new Uri("file://108.png", UriKind.Absolute);

        Assert.False(Uri.TryCreate("file://108.png", UriKind.Relative, out _));

        var uri3 = new Uri("file://108.png", UriKind.RelativeOrAbsolute);
        var uri4 = new Uri("file://images/108.png", UriKind.Absolute);
        Assert.False(Uri.TryCreate("file://images/108.png", UriKind.Relative, out _));
        var uri6 = new Uri("file://images/108.png", UriKind.RelativeOrAbsolute);
 
        Assert.Equal("108.png", uri1.GetPath()); 
        Assert.Equal("108.png", uri3.GetPath()); 
        Assert.Equal("images/108.png", uri4.GetPath()); 
        Assert.Equal("images/108.png", uri6.GetPath()); 
            
        Assert.True(File.Exists(uri1.GetPath()));
        Assert.True(File.Exists(uri6.GetPath()));
    }
}
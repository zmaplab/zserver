using System.Runtime.InteropServices;
using Xunit;
using ZMap.Renderer.SkiaSharp.Utilities;

namespace ZServer.Tests;

public class FontManagerTest
{
    [Fact]
    public void Load()
    {
        FontUtility.Load();
    }

    [Fact]
    public void GetFontByFamilyName()
    {
        FontUtility.Load();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var f1 = FontUtility.Get("Academy Engraved LET", "Al Bayan");
            var f2 = FontUtility.Get("Klee", "Academy Engraved LET");
            Assert.Equal("Academy Engraved LET", f1.FamilyName);
            Assert.Equal("Klee", f2.FamilyName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var f1 = FontUtility.Get("Arial", "Calibri");
            var f2 = FontUtility.Get("微软雅黑", "Segoe");
            Assert.Equal("Arial", f1.FamilyName);
            Assert.Equal("Microsoft YaHei", f2.FamilyName);
        }
    }
}
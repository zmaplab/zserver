using Xunit;
using ZMap.Renderer.SkiaSharp.Utilities;

namespace ZServer.Tests
{
    public class FontManagerTest
    {
        [Fact]
        public void Load()
        {
            FontUtilities.Load();
        }

        [Fact]
        public void GetFontByFamilyName()
        {
            FontUtilities.Load();

            var f1 = FontUtilities.Get("Academy Engraved LET", "Al Bayan");
            var f2 = FontUtilities.Get("Klee", "Academy Engraved LET");
            Assert.Equal("Academy Engraved LET", f1.FamilyName);
            Assert.Equal("Klee", f2.FamilyName);
        }
    }
}
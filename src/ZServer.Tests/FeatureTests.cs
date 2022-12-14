using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap;

namespace ZServer.Tests
{
    public class FeatureTests
    {
        [Fact]
        public void Feature()
        {
            var f = new Feature(new Point(0, 0), new Dictionary<string, dynamic>
            {
                { "geom", null }
            });
            Assert.Null(f["hi"]);
        }
    }
}
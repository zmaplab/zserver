using System;
using System.Collections.Generic;
using System.Linq;
using Natasha.CSharp;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Source;

namespace ZServer.Tests
{
    public class DynamicBuildTests : BaseTests
    {
        public static Func<Feature, bool> EmptyFilter()
        {
            return _ => true;
        }

        [Fact]
        public void QueryShapeFile()
        {
            var f = FastMethodOperator.DefaultDomain().Body("return feature => false;")
                .Return<Func<Feature, bool>>()
                .Compile<Func<Func<Feature, bool>>>();
            var features = new List<Feature>
            {
                new(new Point(0, 0), new Dictionary<string, dynamic>
                {
                    { "geom", null }
                })
            };
            var result = features.Where(f()).ToList();
            Assert.Empty(result);
        }
    }
}
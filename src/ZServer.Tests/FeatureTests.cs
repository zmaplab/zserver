using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Xunit;
using Feature = ZMap.Feature;

namespace ZServer.Tests;

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

    // [Fact]
    // public void FeatureCollectionJsonSerializer()
    // {
    //
    //     var fc = new FeatureCollection
    //     {
    //         new NetTopologySuite.Features.Feature(new Point(1, 2), new AttributesTable()),
    //         new NetTopologySuite.Features.Feature(new Point(3, 5), new AttributesTable())
    //     };
    //
    //     var json = JsonSerializer.Serialize(fc, jsonSerializerOptions);
    // }
}
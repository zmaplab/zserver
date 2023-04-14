using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class GeoUtilitiesTests
{
    [Fact]
    public void CalculateLatLongFromGrid()
    {
        var bbox = new Envelope(117.95745849609375, 117.960205078125, 41.0064697265625, 41.00921630859375);
        var height = 256;
        var width = 256;
        var pixelHeight = (bbox.MaxY - bbox.MinY) / height;
        var pixelWidth = (bbox.MaxX - bbox.MinX) / width;
        GeometryUtilities.CalculateLatLongFromGrid(bbox, pixelWidth, pixelHeight, 183, 208);
        // Assert.Equal(117.95942406089455, latLon.Lon);
        // Assert.Equal(41.006979652534426, latLon.Lat);
    }
}
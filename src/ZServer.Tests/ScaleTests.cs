using NetTopologySuite.Geometries;
using Xunit;
using ZMap.Infrastructure;

namespace ZServer.Tests;

public class ScaleTests : BaseTests
{
    [Fact]
    public void CalculateScale()
    {
        // zoom: 14
        // bbox: 117.3402099609375,31.906367187500003,117.345703125,31.911860351562503
        // width: 1024
        // height: 1024
        // dpi: 180
        // scale: 4231
        // resultScale:   

        Get(117.3402099609375, 31.906367187500003, 117.345703125, 31.911860351562503);

        var scale = GeographicUtilities.CalculateOGCScale(
            Get(117.3402099609375, 31.906367187500003, 117.345703125, 31.911860351562503), 1024, 180);
        Assert.Equal(4231, (int)scale);
    }

    Envelope Get(double x1, double y1, double x2, double y2)
    {
        return new Envelope(x1, x2, y1, y2);
    }
}
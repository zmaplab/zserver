using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using Xunit;
using ZMap.Infrastructure;
using ZMap.Ogc.Wms;

namespace ZServer.Tests;

public class ScaleTests : BaseTests
{
    [Fact]
    public void CalculatePrintScaleRetainsDpiSemantics()
    {
        // zoom: 14
        // bbox: 117.3402099609375,31.906367187500003,117.345703125,31.911860351562503
        // width: 1024
        // height: 1024
        // dpi: 180
        // scale: 4231
        // resultScale:   

        var envelope = Get(117.3402099609375, 31.906367187500003, 117.345703125, 31.911860351562503);
        var scale = GeographicUtility.CalculatePrintScale(
            envelope, 1024, 180);
#pragma warning disable CS0618
        var legacyScale = GeographicUtility.CalculateOGCScale(envelope, 1024, 180);
#pragma warning restore CS0618

        Assert.Equal(scale, legacyScale);
        Assert.Equal(4231, (int)scale);
    }

    [Fact]
    public void CalculateOgcScaleForWebMercatorUsesFixedPixelSize()
    {
        var scale = GeographicUtility.CalculateOGCScaleForSrid(
            new Envelope(0, 1000, 0, 1000), 3857, 1000);

        Assert.Equal(3571.4285714285716D, scale, 10);
    }

    [Fact]
    public void CalculateOgcScaleForGeographicCrsUsesRadiansAndEquatorialRadius()
    {
        var scale = GeographicUtility.CalculateOGCScaleForSrid(
            new Envelope(0, 1, 0, 1), 4326, 1000);
        var expected = 6378137D * Math.PI / 180D / (1000D * 0.00028D);

        Assert.Equal(expected, scale, 9);
    }

    [Theory]
    [InlineData(2240)]
    [InlineData(2277)]
    public void CalculateOgcScaleForFootProjectedCrsUsesProjNetUnit(int srid)
    {
        var scale = GeographicUtility.CalculateOGCScaleForSrid(
            new Envelope(0, 1000, 0, 1000), srid, 1000);
        var expected = 1000D * 0.3048006096012192D / (1000D * 0.00028D);

        Assert.Equal(expected, scale, 10);
    }

    [Fact]
    public void CalculateOgcScaleDoesNotDependOnPrintDpi()
    {
        var envelope = new Envelope(0, 1000, 0, 1000);

        var scale = GeographicUtility.CalculateOGCScaleForSrid(envelope, 3857, 1000);
        var printScaleAt96Dpi = GeographicUtility.CalculatePrintScale(envelope, 1000, 96);
        var printScaleAt180Dpi = GeographicUtility.CalculatePrintScale(envelope, 1000, 180);

        Assert.NotEqual(printScaleAt96Dpi, printScaleAt180Dpi);
        Assert.Equal(3571.4285714285716D, scale, 10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateOgcScaleRejectsNonPositiveImageWidth(int imageWidth)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(new Envelope(0, 1000, 0, 1000), 3857, imageWidth));
    }

    [Fact]
    public void CalculateOgcScaleRejectsUnknownSrid()
    {
        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(new Envelope(0, 1, 0, 1), 999999, 1000));
    }

    [Fact]
    public void CalculateOgcScaleRejectsNonTwoDimensionalCrs()
    {
        const int testSrid = 999998;
        CoordinateReferenceSystem.SRIDCache.TryGetValue(testSrid, out var previous);
        CoordinateReferenceSystem.SRIDCache[testSrid] = GeocentricCoordinateSystem.WGS84;

        try
        {
            Assert.Throws<ArgumentException>(() =>
                GeographicUtility.CalculateOGCScaleForSrid(new Envelope(0, 1000, 0, 1000), testSrid, 1000));
        }
        finally
        {
            if (previous == null)
            {
                CoordinateReferenceSystem.SRIDCache.Remove(testSrid);
            }
            else
            {
                CoordinateReferenceSystem.SRIDCache[testSrid] = previous;
            }
        }
    }

    [Fact]
    public void CalculateOgcScaleRejectsProjectedCrsWithoutLinearUnit()
    {
        const int testSrid = 999997;
        CoordinateReferenceSystem.SRIDCache.TryGetValue(testSrid, out var previous);
        var coordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(1, true);
        coordinateSystem.LinearUnit = null;
        CoordinateReferenceSystem.SRIDCache[testSrid] = coordinateSystem;

        try
        {
            Assert.Throws<ArgumentException>(() =>
                GeographicUtility.CalculateOGCScaleForSrid(new Envelope(0, 1000, 0, 1000), testSrid, 1000));
        }
        finally
        {
            if (previous == null)
            {
                CoordinateReferenceSystem.SRIDCache.Remove(testSrid);
            }
            else
            {
                CoordinateReferenceSystem.SRIDCache[testSrid] = previous;
            }
        }
    }

    [Fact]
    public void CalculateOgcScaleRejectsNullEnvelope()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(null, 3857, 1000));
    }

    [Fact]
    public void CalculateOgcScaleRejectsNullEnvelopeBeforeCrsLookup()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(null, 999999, 1000));
    }

    [Fact]
    public void CalculateOgcScaleForGeographicCrsUsesRegisteredAngularUnit()
    {
        var coordinateSystem = CoordinateReferenceSystem.Get(4326) as GeographicCoordinateSystem;

        Assert.NotNull(coordinateSystem);
        Assert.True(coordinateSystem.AngularUnit.RadiansPerUnit > 0);
    }

    [Fact]
    public void CalculateOgcScaleForSridRequiresAValidEnvelope()
    {
        var envelope = new Envelope();

        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(envelope, 3857, 1000));
    }

    [Theory]
    [MemberData(nameof(NonFiniteEnvelopeCases))]
    public void CalculateOgcScaleRejectsNonFiniteEnvelopeBoundaries(Envelope envelope)
    {
        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(envelope, 3857, 1000));
    }

    public static IEnumerable<object[]> NonFiniteEnvelopeCases()
    {
        yield return new object[] { new Envelope(double.PositiveInfinity, 1, 0, 1) };
        yield return new object[] { new Envelope(0, double.PositiveInfinity, 0, 1) };
        yield return new object[] { new Envelope(0, 1, double.NegativeInfinity, double.PositiveInfinity) };
        yield return new object[] { new Envelope(0, 1, double.PositiveInfinity, 0) };
        yield return new object[] { new Envelope(0, 1, 0, double.PositiveInfinity) };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CalculateOgcScaleRejectsNonPositiveEnvelopeDimensions(bool zeroWidth)
    {
        var envelope = zeroWidth
            ? new Envelope(10, 10, 0, 1)
            : new Envelope(0, 1, 10, 10);

        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(envelope, 3857, 1000));
    }

    [Fact]
    public void CalculateOgcScaleRejectsNonFiniteComputedScale()
    {
        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(
                new Envelope(0, double.MaxValue, 0, 1), 4326, 1000));
    }

    [Fact]
    public void CalculateOgcScaleRejectsScaleThatUnderflowsToZero()
    {
        Assert.Throws<ArgumentException>(() =>
            GeographicUtility.CalculateOGCScaleForSrid(
                new Envelope(0, double.Epsilon, 0, 1), 3857, int.MaxValue));
    }

    [Theory]
    [InlineData("NaN,0,1,1")]
    [InlineData("0,NaN,1,1")]
    [InlineData("0,0,NaN,1")]
    [InlineData("0,0,1,NaN")]
    [InlineData("Infinity,0,1,1")]
    [InlineData("0,Infinity,1,1")]
    [InlineData("0,0,Infinity,1")]
    [InlineData("0,0,1,Infinity")]
    public void GetMapValidatorRejectsNonFiniteBboxCoordinates(string bbox)
    {
        var result = ParameterValidator.VerifyAndBuildWmsGetMapArguments(
            "zserver:layer", "EPSG:3857", bbox, 256, 256, "image/png", null, null);

        Assert.Equal("InvalidBBox", result.Code);
        Assert.Null(result.Parameters);
    }

    [Theory]
    [InlineData("0,0,0,1")]
    [InlineData("0,0,1,0")]
    [InlineData("1,0,0,1")]
    [InlineData("0,1,1,1")]
    public void GetMapValidatorRejectsNonPositiveBboxRange(string bbox)
    {
        var result = ParameterValidator.VerifyAndBuildWmsGetMapArguments(
            "zserver:layer", "EPSG:3857", bbox, 256, 256, "image/png", null, null);

        Assert.Equal("InvalidBBox", result.Code);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void GetMapValidatorRejectsBboxRangeOverflow()
    {
        var result = ParameterValidator.VerifyAndBuildWmsGetMapArguments(
            "zserver:layer", "EPSG:3857", "-1.7E308,0,1.7E308,1", 256, 256, "image/png", null, null);

        Assert.Equal("InvalidBBox", result.Code);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void GetMapValidatorPreservesFiniteBboxPrecision()
    {
        var result = ParameterValidator.VerifyAndBuildWmsGetMapArguments(
            "zserver:layer", "EPSG:3857",
            "0.12345678901234567,0.25,0.22345678901234567,0.75",
            256, 256, "image/png", null, null);

        Assert.Null(result.Code);
        Assert.Equal(0.12345678901234567D, result.Parameters.Envelope.MinX);
        Assert.Equal(0.22345678901234567D, result.Parameters.Envelope.MaxX);
    }

    [Fact]
    public async Task GetMapRejectsNonFiniteBboxBeforeMapExecution()
    {
        var service = new WmsService(null, null, null, null);

        using var result = await service.GetMapAsync(
            "zserver:layer", null, "EPSG:3857", "NaN,0,1,1", 256, 256, "image/png",
            false, null, 0, "dpi:180", null, new Dictionary<string, object>());

        Assert.Equal("InvalidBBox", result.Code);
    }

    Envelope Get(double x1, double y1, double x2, double y2)
    {
        return new Envelope(x1, x2, y1, y2);
    }
}

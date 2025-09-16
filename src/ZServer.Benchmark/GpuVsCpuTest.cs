using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SkiaSharp;
using ZMap.Renderer.SkiaSharp;
using ZMap.Renderer.SkiaSharp.GlContexts;
using ZMap.Style;
using Feature = ZMap.Feature;

namespace ZServer.Benchmark;

public class GpuVsCpuTest
{
    public GpuVsCpuTest()
    {
        GlContextHelper.Initialize();
    }

    [Benchmark]
    public void Gpu()
    {
        var data = GetFeatures();

        var style = new FillStyle
        {
            Antialias = true,
            Opacity = CSharpExpressionV2.Create<float?>("1"),
            Color = CSharpExpressionV2.Create<string>("#3ed53e")
        };

        using var bitmap = new SKBitmap(256, 256);

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var width = 256;
        var height = 256;

        using var graphicsService = new SkiaGraphicsService(width, height);

        foreach (var feature in data)
        {
            graphicsService.Render(_extent, feature.Geometry, style);
        }

        graphicsService.GetImage("image/png");
    }

    // [Benchmark]
    // public void Cpu()
    // {
    //     var data = GetFeatures();
    //
    //     var style = new FillStyle
    //     {
    //         Antialias = true,
    //         Opacity = CSharpExpressionV2.Create<float?>("1"),
    //         Color = CSharpExpressionV2.Create<string>("#3ed53e")
    //     };
    //
    //     using var bitmap = new SKBitmap(256, 256);
    //
    //     using var canvas = new SKCanvas(bitmap);
    //     canvas.Clear(SKColors.White);
    //
    //     var width = 256;
    //     var height = 256;
    //
    //     using var graphicsService =
    //         new SkiaGraphicsService(width, height, false);
    //
    //     foreach (var feature in data)
    //     {
    //         graphicsService.Render(_extent, feature.Geometry, style);
    //     }
    //
    //     graphicsService.GetImage("image/png");
    // }

    private Feature ToDictionary(IFeature feature)
    {
        var dict = new Dictionary<string, object>();
        foreach (var name in feature.Attributes.GetNames())
        {
            dict.Add(name, feature.Attributes[name]);
        }

        return new Feature(feature.Geometry, dict);
    }

    private FeatureCollection GetGeometries(string path)
    {
        var json = File.ReadAllText(path);
        var reader = new GeoJsonReader();
        var collection = reader.Read<FeatureCollection>(json);
        return collection;
    }

    private List<Feature> GetFeatures()
    {
        var c = GetGeometries("polygons.json");
        return c.Select(ToDictionary).ToList();
    }

    private readonly Envelope _extent = new(-160.9, 105, -75, 103);
}
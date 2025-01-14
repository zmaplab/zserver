using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
using ZServer.Store;
using Feature = ZMap.Feature;

namespace ZServer.Tests;

public abstract class BaseTests
{
    protected readonly Envelope Extent = new(-160.9, 105, -75, 103);
    private static readonly IServiceProvider Service;

    static BaseTests()
    {
        CSharpDynamicCompiler.Load<NatashaDynamicCompiler>();
        var serviceCollection = new ServiceCollection();
        var configuration = GetConfiguration();
        serviceCollection.AddMemoryCache();
        serviceCollection.AddLogging(x => x.AddConsole());
        serviceCollection.AddZServer(configuration, "layers.json");
        serviceCollection.TryAddSingleton(configuration);
        serviceCollection.TryAddSingleton<StoreRefreshService>();
        Service = serviceCollection.BuildServiceProvider();
        var syncService = Service.GetRequiredService<StoreRefreshService>();
        var configurationProvider = Service.GetRequiredService<JsonStoreProvider>();

        syncService.RefreshAsync(configurationProvider).GetAwaiter().GetResult();
    }

    // protected IRendererFactory GetRendererFactory()
    // {
    //     return GetScopedService<IRendererFactory>();
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


    protected List<Feature> GetFeatures()
    {
        var c = GetGeometries("polygons.json");
        return c.Select(ToDictionary).ToList();
    }

    protected List<Feature> GetFeatures(string path)
    {
        var c = GetGeometries(path);
        return c.Select(ToDictionary).ToList();
    }

    protected BaseTests()
    {
        if (!Directory.Exists("images"))
        {
            Directory.CreateDirectory("images");
        }
    }

    private FeatureCollection GetGeometries(string path)
    {
        var json = File.ReadAllText(path);
        var reader = new GeoJsonReader();
        var collection = reader.Read<FeatureCollection>(json);
        return collection;
    }

    private FeatureCollection GetMultiLine()
    {
        var json = File.ReadAllText("polygons.json");
        var reader = new GeoJsonReader();
        var collection = reader.Read<FeatureCollection>(json);
        return collection;
    }

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json");
        return builder.Build();
    }

    protected T GetScopedService<T>()
    {
        return Service.CreateScope().ServiceProvider.GetService<T>();
    }

    protected T GetService<T>()
    {
        return Service.GetService<T>();
    }
}
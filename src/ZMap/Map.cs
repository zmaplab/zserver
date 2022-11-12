using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;

namespace ZMap;

public class Map : IDisposable
{
    private ILogger _logger = NullLogger.Instance;
    private readonly List<ILayer> _layers = new();
    private IGraphicsServiceProvider _graphicsServiceProvider;
    private Zoom _zoom;
    private int _srid;

    public IReadOnlyCollection<ILayer> Layers => _layers;
    public string Id { get; private set; }

    public Map SetZoom(Zoom zoom)
    {
        _zoom = zoom;
        return this;
    }

    public Map SetSRID(int srid)
    {
        _srid = srid;
        return this;
    }

    public Map SetId(string id)
    {
        Id = id;
        return this;
    }

    public Map SetGraphicsContextFactory(IGraphicsServiceProvider graphicsServiceProvider)
    {
        _graphicsServiceProvider = graphicsServiceProvider;
        return this;
    }

    public Map AddLayers(IEnumerable<ILayer> layers)
    {
        foreach (var layer in layers)
        {
            _layers.Add(layer);
        }

        return this;
    }

    public Map SetLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public async Task<byte[]> GetImageAsync(Viewport viewport, string imageFormat)
    {
        if (viewport.Extent == null || viewport.Extent.IsNull)
        {
            return Array.Empty<byte>();
        }

        using var graphicsService =
            _graphicsServiceProvider.Create(Id, viewport.Width, viewport.Height);
        if (graphicsService == null)
        {
            _logger.LogWarning("Create graphics context failed");
            return Array.Empty<byte>();
        }

        for (var i = _layers.Count - 1; i >= 0; --i)
        {
            var layer = _layers[i];
            await layer.RenderAsync(graphicsService, viewport, _zoom, _srid);
        }

        return graphicsService.GetImage(imageFormat, viewport.Bordered);
    }

    public void Dispose()
    {
    }
}
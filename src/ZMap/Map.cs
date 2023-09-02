using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ZMap;

public class Map : IDisposable
{
    private ILogger _logger = NullLogger.Instance;
    private readonly List<Layer> _layers = new();
    private IGraphicsServiceProvider _graphicsServiceProvider;
    private Zoom _zoom;
    private int _srid;
    public IReadOnlyCollection<Layer> Layers => _layers;
    public string Id { get; private set; }

    public Map SetZoom(Zoom zoom)
    {
        _zoom = zoom;
        return this;
    }

    public Map SetSrid(int srid)
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

    public Map AddLayers(IEnumerable<Layer> layers)
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

    public async Task<Stream> GetImageAsync(Viewport viewport, string imageFormat)
    {
        if (viewport.Extent == null || viewport.Extent.IsNull)
        {
            return Stream.Null;
        }

        using var graphicsService =
            _graphicsServiceProvider.Create(Id, viewport.Width, viewport.Height);
        if (graphicsService == null)
        {
            _logger.LogWarning("创建图形服务失败");
            return Stream.Null;
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
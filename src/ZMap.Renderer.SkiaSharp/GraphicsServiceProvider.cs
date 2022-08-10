using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace ZMap.Renderer.SkiaSharp;

public class GraphicsServiceProvider : IGraphicsServiceProvider
{
    private readonly IMemoryCache _memoryCache;

    public GraphicsServiceProvider(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public IGraphicsService Create(string mapId, int width, int height, Envelope envelope)
    {
        return new SkiaGraphicsService(mapId, width, height, envelope, _memoryCache);
    }
}
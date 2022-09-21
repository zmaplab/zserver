using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace ZMap.Renderer.SkiaSharp;

public class GraphicsServiceProvider : IGraphicsServiceProvider
{
    public IGraphicsService Create(string mapId, int width, int height)
    {
        return new SkiaGraphicsService(mapId, width, height);
    }
}
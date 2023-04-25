using System;
using NetTopologySuite.Geometries;
using ZMap.Style;

namespace ZMap;

public interface IGraphicsService : IDisposable
{
    string Identifier { get; }
    int Width { get; }
    int Height { get; }
    byte[] GetImage(string imageFormat, bool bordered = false);
    void Render(byte[] image, RasterStyle style);
    void Render(Envelope extent, Geometry geometry, VectorStyle style);
}
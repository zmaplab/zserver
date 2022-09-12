using System;
using NetTopologySuite.Geometries;
using ZMap.Source;
using ZMap.Style;

namespace ZMap;

public interface IGraphicsService : IDisposable
{
    string MapId { get; }
    int Width { get; }
    int Height { get; }
    byte[] GetImage(string imageFormat);
    void Render(byte[] image, Style.RasterStyle style);
    void Render(Envelope extent, Feature feature, VectorStyle style);
}
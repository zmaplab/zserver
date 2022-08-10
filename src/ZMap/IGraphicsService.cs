using System;
using NetTopologySuite.Geometries;
using ZMap.Source;

namespace ZMap;

public interface IGraphicsService : IDisposable
{
    string MapId { get; }
    int Width { get; }
    int Height { get; }
    Envelope Envelope { get; }
    byte[] GetImage(string imageFormat);
    void Render(Style.RasterStyle style, byte[] image);
    void Render(Style.VectorStyle style, Feature feature);
}
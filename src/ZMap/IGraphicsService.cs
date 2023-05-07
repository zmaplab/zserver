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

    /// <summary>
    /// 渲染影像数据
    /// </summary>
    /// <param name="image"></param>
    /// <param name="style"></param>
    void Render(byte[] image, RasterStyle style);

    /// <summary>
    /// 渲染矢量数据
    /// </summary>
    /// <param name="extent"></param>
    /// <param name="geometry"></param>
    /// <param name="style"></param>
    void Render(Envelope extent, Geometry geometry, VectorStyle style);
}
using System;
using System.IO;
using NetTopologySuite.Geometries;
using ZMap.Style;

namespace ZMap;

public interface IGraphicsService : IDisposable
{
    /// <summary>
    /// 应用跟踪标识符
    /// </summary>
    string TraceIdentifier { get; set; }

    /// <summary>
    /// 渲染宽度
    /// </summary>
    int Width { get; }

    /// <summary>
    /// 渲染高度
    /// </summary>
    int Height { get; }

    /// <summary>
    /// 获取渲染的图像
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <param name="bordered"></param>
    /// <returns></returns>
    Stream GetImage(string imageFormat, bool bordered = false);

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
namespace ZMap.Renderer;

// public interface IVectorRenderer<in TGraphics> : IRenderer<TGraphics, Geometry>;

/// <summary>
/// 矢量数据绘制器
/// </summary>
/// <typeparam name="TGraphics"></typeparam>
public interface IVectorRenderer<in TGraphics> : IRenderer
{
    void Render(TGraphics graphics, Geometry content, Envelope extent, int width, int height);
}
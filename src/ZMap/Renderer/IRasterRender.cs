namespace ZMap.Renderer;

/// <summary>
/// 栅格数据绘制器
/// </summary>
/// <typeparam name="TGraphics"></typeparam>
public interface IRasterRender<in TGraphics> : IRenderer
{
    void Render(TGraphics graphics, Envelope content, Envelope extent, int width, int height, byte[] image);
}
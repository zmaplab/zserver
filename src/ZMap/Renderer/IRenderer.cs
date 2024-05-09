namespace ZMap.Renderer;

public interface IRenderer;

public interface IRenderer<in TGraphics, in TContent> : IRenderer
{
    void Render(TGraphics graphics, TContent content, Envelope extent, int width, int height);
}
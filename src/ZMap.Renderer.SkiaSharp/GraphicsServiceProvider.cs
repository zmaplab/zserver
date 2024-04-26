namespace ZMap.Renderer.SkiaSharp;

public class GraphicsServiceProvider : IGraphicsServiceProvider
{
    public IGraphicsService Create(int width, int height)
    {
        return new SkiaGraphicsService(width, height);
    }
}
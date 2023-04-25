namespace ZMap.Renderer.SkiaSharp;

public class GraphicsServiceProvider : IGraphicsServiceProvider
{
    public IGraphicsService Create(string identifier, int width, int height)
    {
        return new SkiaGraphicsService(identifier, width, height);
    }
}
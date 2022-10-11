namespace ZMap;

public interface IGraphicsServiceProvider
{
    IGraphicsService Create(string mapId, int width, int height);
}
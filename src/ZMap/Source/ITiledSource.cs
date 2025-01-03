using ZMap.TileGrid;

namespace ZMap.Source;

public interface ITiledSource : ISource
{
    GridSet GridSet { get; set; }
    Task<ImageData> GetImageAsync(string matrix, int row, int col);
}
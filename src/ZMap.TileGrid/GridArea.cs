// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ZMap.TileGrid;

public class GridArea
{
    public string Level { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    public bool IsEmpty => MinX < 0 && MaxX < 0 && MinY < 0 && MaxY < 0;
}
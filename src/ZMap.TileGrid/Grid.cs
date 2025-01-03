// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ZMap.TileGrid;

public class Grid(int index)
{
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public int Index { get; private set; } = index;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 横向瓦片数量
    /// </summary>
    public int NumTilesWidth { get; set; }

    /// <summary>
    /// 纵向瓦片数量
    /// </summary>
    public int NumTilesHeight { get; set; }

    /// <summary>
    /// 分辩率
    /// </summary>
    public double Resolution { get; set; }

    /// <summary>
    /// 比例尺分母
    /// </summary>
    public double ScaleDenominator { get; set; }

    public int Height { get; set; }
    public int Width { get; set; }
    public int TileHeight { get; set; }
    public int TileWidth { get; set; }

    public override string ToString()
    {
        return $"{Name}: {Resolution}, {ScaleDenominator}";
    }
}
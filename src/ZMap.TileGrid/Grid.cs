namespace ZMap.TileGrid;

public class Grid
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// 横向瓦片数量
    /// </summary>
    public long NumTilesWide { get; set; }
        
    /// <summary>
    /// 纵向瓦片数量
    /// </summary>
    public long NumTilesHigh { get; set; }

    /// <summary>
    /// 分辩率
    /// </summary>
    public double Resolution { get; set; }

    /// <summary>
    /// 比例尺分母
    /// </summary>
    public double ScaleDenominator { get; set; }

    public override string ToString()
    {
        return $"{Name}: {Resolution}, {ScaleDenominator}";
    }
}
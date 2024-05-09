namespace ZMap.Indexing;

[Serializable]
[MessagePackObject]
public record SpatialIndexItem
{
    /// <summary>
    /// 
    /// </summary>
    [Key(0)]
    public double X1 { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Key(1)]
    public double X2 { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Key(2)]
    public double Y1 { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Key(3)]
    public double Y2 { get; set; }

    /// <summary>
    /// 数据在索引树中的位置
    /// </summary>
    [Key(4)]
    public uint Index { get; set; }

    /// <summary>
    /// 数据在整个文件流中的起始位置
    /// </summary>
    [Key(5)]
    public int Offset { get; set; }

    /// <summary>
    /// 数据长度
    /// </summary>
    [Key(6)]
    public int Length { get; set; }
}
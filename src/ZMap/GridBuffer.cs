namespace ZMap;

/// <summary>
/// 用于 不同比例尺下使用不同的缓冲大小
/// </summary>
public class GridBuffer : IVisibleLimit
{
    public double MinZoom { get; set; }
    public double MaxZoom { get; set; }
    public ZoomUnits ZoomUnit { get; set; }
    public int Size { get; set; }
}
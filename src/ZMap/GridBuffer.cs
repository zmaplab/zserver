namespace ZMap;

public class GridBuffer : IVisibleLimit
{
    public double MinZoom { get; set; }
    public double MaxZoom { get; set; }
    public ZoomUnits ZoomUnit { get; set; }
    public int Size { get; set; }
}
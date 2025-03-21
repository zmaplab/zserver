// ReSharper disable InconsistentNaming

namespace ZMap;

public class Viewport
{
    public Tile Tile { get; set; }
    public Envelope Extent { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Transparent { get; set; }
    public string BackgroundColor { get; set; }
    public bool Bordered { get; set; }
}
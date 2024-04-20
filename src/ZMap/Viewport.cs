using NetTopologySuite.Geometries;

// ReSharper disable InconsistentNaming

namespace ZMap;

public class Viewport
{
    public Envelope Extent { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Transparent { get; set; }
    public bool Bordered { get; set; }
}
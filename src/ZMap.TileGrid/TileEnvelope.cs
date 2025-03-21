using NetTopologySuite.Geometries;

namespace ZMap.TileGrid;

public class TileEnvelope
{
    public Envelope Extent { get; set; }
    public double ScaleDenominator { get; set; }
}
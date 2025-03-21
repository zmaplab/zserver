namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoKeyEntry
{
    public int KeyId { get; set; }
    public int Location { get; set; }
    public int Count { get; set; }
    public int ValueOrOffset { get; set; }
    public dynamic Value { get; set; }
}
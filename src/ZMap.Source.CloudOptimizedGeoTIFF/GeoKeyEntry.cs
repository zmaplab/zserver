namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoKeyEntry
{
    public int KeyId { get; set; }
    public int TiffTagLocation { get; set; }
    public int Count { get; set; }
    public int ValueOffset { get; set; }
    public dynamic Value { get; set; }
}
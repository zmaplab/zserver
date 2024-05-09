namespace ZMap;

public static class Defaults
{
    /// <summary>
    /// Conversion factor degrees to radians
    /// </summary>
    public const double DegToRad = Math.PI / 180d; //0.01745329252; // Convert Degrees to Radians

    /// <summary>
    /// Meters per degree at equator
    /// </summary>
    public const double MetersPerDegreeAtEquator = MetersPerMile * MilesPerDegreeAtEquator;

    /// <summary>
    /// Meters per mile
    /// </summary>
    public const double MetersPerMile = 1609.347219;

    /// <summary>
    /// Miles per degree at equator
    /// </summary>
    public const double MilesPerDegreeAtEquator = 69.171;

    public const double MaxZoomValue = 591658711;
    public const string WmsScaleKey = "wms_scale_denominator";
    
    public const string TraceIdentifier = "TraceIdentifier";
    public const string AdditionalFilter = "AdditionalFilter";
}
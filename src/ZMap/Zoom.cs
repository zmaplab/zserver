namespace ZMap;

public record Zoom(double Value, ZoomUnits Units)
{
    public readonly double Value = Value;
    public readonly ZoomUnits Units = Units;
}
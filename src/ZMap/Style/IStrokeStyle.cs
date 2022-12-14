namespace ZMap.Style;

public interface IStrokeStyle
{
    Expression<float> Opacity { get; set; }
    Expression<int> Width { get; set; }
    Expression<string> Color { get; set; }
    Expression<float[]> DashArray { get; set; }
    Expression<float> DashOffset { get; set; }
    Expression<string> LineJoin { get; set; }
    Expression<string> LineCap { get; set; }
}
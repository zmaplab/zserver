namespace ZMap.Style;

public interface IFillStyle
{
    Expression<float> Opacity { get; set; }
    Expression<string> Color { get; set; }
    Expression<double[]> Translate { get; set; }
}
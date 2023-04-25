namespace ZMap.Style;

public interface IStrokeStyle
{
    CSharpExpression<float> Opacity { get; set; }
    CSharpExpression<int> Width { get; set; }
    CSharpExpression<string> Color { get; set; }
    CSharpExpression<float[]> DashArray { get; set; }
    CSharpExpression<float> DashOffset { get; set; }
    CSharpExpression<string> LineJoin { get; set; }
    CSharpExpression<string> LineCap { get; set; }
}
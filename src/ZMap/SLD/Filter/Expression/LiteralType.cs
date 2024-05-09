namespace ZMap.SLD.Filter.Expression;

[XmlRoot("Literal")]
public class LiteralType : ExpressionType
{
    /// <summary>
    /// 
    /// </summary>
    [XmlText]
    public string Value { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Push(CSharpExpressionV2.Create<string>(Value));
        return null;
    }
}
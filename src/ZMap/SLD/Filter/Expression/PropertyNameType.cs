using System.Xml.Serialization;

namespace ZMap.SLD.Filter.Expression;

[XmlRoot("PropertyName")]
public class PropertyNameType : ExpressionType
{
    /// <summary>
    /// 
    /// </summary>
    [XmlText]
    public string Text { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Push(ZMap.Style.Expression.New($"feature[\"{Text.Trim()}\"]"));
        return null;
    }
}
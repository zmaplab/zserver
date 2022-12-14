using System.Xml.Serialization;

namespace ZMap.SLD.Expression;

public class Literal : Expression
{
    /// <summary>
    /// 
    /// </summary>
    [XmlText]
    public string Value { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        return visitor.Visit(this, extraData);
    }

    public override object Evaluate(object @object)
    {
        return Value;
    }

    public override T Evaluate<T>(object @object)
    {
        throw new System.NotImplementedException();
    }

 
}
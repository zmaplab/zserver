using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD.Expression;

public class PropertyName : Expression
{
    /// <summary>
    /// 
    /// </summary>
    [XmlText]
    public string Name { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Push(ZMap.Style.Expression<string>.New(null, $"feature[\"{Name.Trim()}\"]"));
        return null;
    }

    public override object Evaluate(object @object)
    {
        throw new System.NotImplementedException();
    }

    public override T Evaluate<T>(object @object)
    {
        throw new System.NotImplementedException();
    }
}
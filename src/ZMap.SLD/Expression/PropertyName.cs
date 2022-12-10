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
        var dict = extraData as IDictionary<string, object>;
        if (dict == null)
        {
            return null;
        }

        return dict.ContainsKey(Name) ? dict[Name] : null;
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
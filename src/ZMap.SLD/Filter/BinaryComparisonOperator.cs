using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

public abstract class BinaryComparisonOperator : Filter
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("PropertyName")]
    [XmlElement("Literal")]
    [XmlElement("Add")]
    public List<Expression.Expression> Expressions { get; set; }

    public Expression.Expression Left => Expressions.ElementAtOrDefault(0);
    public Expression.Expression Right => Expressions.ElementAtOrDefault(1);
}
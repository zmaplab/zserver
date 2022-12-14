using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZMap.SLD.Expression;

namespace ZMap.SLD.Filter;

public abstract class BinaryComparisonOperator : Filter
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("PropertyName", Type = typeof(PropertyName))]
    [XmlElement("Literal", Type = typeof(Literal))]
    [XmlElement("Add", Type = typeof(Add))]
    public List<Expression.Expression> Expressions { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlIgnore]
    public Expression.Expression Left => Expressions.ElementAtOrDefault(0);

    /// <summary>
    /// 
    /// </summary>
    [XmlIgnore]
    public Expression.Expression Right => Expressions.ElementAtOrDefault(1);
}
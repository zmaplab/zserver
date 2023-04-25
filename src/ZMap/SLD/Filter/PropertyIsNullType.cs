using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("PropertyIsNull")]
public class PropertyIsNullType : ComparisonOpsType
{
    /// <remarks/>
    public PropertyNameType PropertyName { get; set; }

    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        visitor.VisitObject(PropertyName, extraData);

        var expression = (ZMap.Style.CSharpExpression)visitor.Pop();

        visitor.Push(ZMap.Style.CSharpExpression.New($"{expression.Body} == null"));

        return null;
    }
}
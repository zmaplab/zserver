using System.Xml.Serialization;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter;

/// <remarks/>
[System.SerializableAttribute]
[XmlType]
[XmlRoot("PropertyIsEmpty")]
public class PropertyIsEmptyType : ComparisonOpsType
{
    /// <remarks/>
    public PropertyNameType PropertyName { get; set; }

    public override object Accept(IFilterVisitor visitor, object extraData)
    {
        visitor.VisitObject(PropertyName, extraData);

        var expression = (ZMap.Style.Expression)visitor.Pop();

        visitor.Push(ZMap.Style.Expression.New($"string.IsNullOrWhiteSpace({expression.Body}?.ToString())"));

        return null;
    }
}
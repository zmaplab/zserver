using System.Xml.Serialization;
using ZMap.Style;

namespace ZMap.SLD.Filter;

[XmlRoot("Filter")]
public class FilterType
{
    [XmlElement("And", typeof(And))]
    [XmlElement("FeatureId", typeof(FeatureIdType))]
    [XmlElement("Not", typeof(Not))]
    [XmlElement("Or", typeof(Or))]
    [XmlElement("PropertyIsBetween", typeof(PropertyIsBetweenType))]
    [XmlElement("PropertyIsEqualTo", typeof(PropertyIsEqualToType))]
    [XmlElement("PropertyIsGreaterThan", typeof(PropertyIsGreaterThanType))]
    [XmlElement("PropertyIsGreaterThanOrEqualTo", typeof(PropertyIsGreaterThanOrEqualTo))]
    [XmlElement("PropertyIsLessThan", typeof(PropertyIsLessThan))]
    [XmlElement("PropertyIsLessThanOrEqualTo", typeof(PropertyIsLessThanOrEqualTo))]
    [XmlElement("PropertyIsLike", typeof(PropertyIsLikeType))]
    [XmlElement("PropertyIsNotEqualTo", typeof(PropertyIsNotEqualTo))]
    [XmlElement("PropertyIsNull", typeof(PropertyIsNullType))]
    [XmlElement("PropertyIsEmpty", typeof(PropertyIsEmptyType))]
    [XmlElement("_Id", typeof(AbstractIdType))]
    [XmlElement("comparisonOps", typeof(ComparisonOpsType))]
    [XmlElement("logicOps", typeof(LogicOpsType))]
    [XmlChoiceIdentifier("ItemsElementName")]
    public object[] Items { get; set; }

    [XmlIgnore, XmlElement("ItemsElementName")]
    public FilterItemsChoiceType[] ItemsElementName { get; set; }

    public virtual object Accept(IFilterVisitor visitor, object extraData)
    {
        if (Items == null || Items.Length == 0)
        {
            visitor.Push(default(ZMap.Style.Expression));
            return null;
        }

        var item = Items[0];
        visitor.VisitObject(item, extraData);

        return null;
    }

    public enum FilterItemsChoiceType
    {
        /// <remarks/>
        And,

        /// <remarks/>
        FeatureId,

        /// <remarks/>
        Not,

        /// <remarks/>
        Or,

        /// <remarks/>
        PropertyIsBetween,

        /// <remarks/>
        PropertyIsEqualTo,

        /// <remarks/>
        PropertyIsGreaterThan,

        /// <remarks/>
        PropertyIsGreaterThanOrEqualTo,

        /// <remarks/>
        PropertyIsLessThan,

        /// <remarks/>
        PropertyIsLessThanOrEqualTo,

        /// <remarks/>
        PropertyIsLike,

        /// <remarks/>
        PropertyIsNotEqualTo,

        /// <remarks/>
        PropertyIsNull,

        PropertyIsEmpty,

        /// <remarks/>
        _Id,

        /// <remarks/>
        comparisonOps,

        /// <remarks/>
        logicOps,
    }
}
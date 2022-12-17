using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

[XmlRoot("And")]
public class BinaryLogicOpType : LogicOpsType
{
    /// <remarks/>
    /// <remarks/>
    [XmlElement("And", typeof(And))]
    [XmlElement("Not", typeof(Not))]
    [XmlElement("Or", typeof(Or))]
    [XmlElement("PropertyIsBetween", typeof(PropertyIsBetweenType))]
    [XmlElement("PropertyIsEqualTo", typeof(PropertyIsEqualToType))]
    [XmlElement("PropertyIsGreaterThan", typeof(PropertyIsGreaterThanType))]
    [XmlElement("PropertyIsGreaterThanOrEqualTo",
        typeof(PropertyIsGreaterThanOrEqualTo))]
    [XmlElement("PropertyIsLessThan", typeof(PropertyIsLessThan))]
    [XmlElement("PropertyIsLessThanOrEqualTo", typeof(PropertyIsLessThanOrEqualTo))]
    [XmlElement("PropertyIsLike", typeof(PropertyIsLikeType))]
    [XmlElement("PropertyIsNotEqualTo", typeof(PropertyIsNotEqualTo))]
    [XmlElement("PropertyIsNull", typeof(PropertyIsNullType))]
    [XmlElement("PropertyIsEmpty", typeof(PropertyIsEmptyType))]
    [XmlElement("comparisonOps", typeof(ComparisonOpsType))]
    [XmlElement("logicOps", typeof(LogicOpsType))]
    [XmlChoiceIdentifier("ItemsElementName")]
    public object[] Items { get; set; }

    /// <remarks/>
    [XmlIgnore]
    [XmlElement("ItemsElementName")]
    public BinaryLogicOpChoiceType[] ItemsElementName { get; set; }

    public enum BinaryLogicOpChoiceType
    {
        /// <remarks/>
        And,

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
        comparisonOps,

        /// <remarks/>
        logicOps,
    }
}
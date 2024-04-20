using System;
using System.Xml.Serialization;

namespace ZMap.SLD;

[XmlInclude(typeof(RasterSymbolizer))]
[XmlInclude(typeof(TextSymbolizer))]
[XmlInclude(typeof(PointSymbolizer))]
[XmlInclude(typeof(PolygonSymbolizer))]
[XmlInclude(typeof(LineSymbolizer))]
[XmlType]
public abstract class Symbolizer
{
    /// <summary>
    /// Returns the name of the geometry feature attribute to use for drawing. May return null (or
    /// Expression.NIL) if this symbol is to use the default geometry attribute, whatever it may be.
    /// Using null in this fashion is similar to a PropertyName using the XPath expression ".".
    /// </summary>
    [XmlElement("Geometry")]
    public string GeometryPropertyName { get; set; }

    /// <summary>
    /// Returns a measure unit. This parameter is inherited from GML. Renderers shall use the unit to
    /// correctly render symbols.
    /// metre | foot | pixel
    /// If the unit is null than we shall use a the pixel unit
    /// </summary>
    [XmlElement("UoM")]
    public string UoM { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [XmlElement("Description")]
    public Description Description { get; set; }

    public virtual object Accept(IStyleVisitor visitor, object extraData)
    {
        throw new NotSupportedException();
    }
}
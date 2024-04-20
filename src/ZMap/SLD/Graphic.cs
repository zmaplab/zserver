using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZMap.SLD;

[XmlRoot("Graphic")]
public class Graphic
{
    /// <summary>
    /// 
    /// </summary>
    public List<GraphicalSymbol> GraphicalSymbols { get; set; }

    /// <summary>
    /// Specifies the opacity (transparency) of the symbol.
    /// Values range from 0 (completely transparent) to 1 (completely opaque).
    /// Value may contain expressions. Default is 1 (opaque).
    /// </summary>
    [XmlElement("Opacity")]
    public ParameterValue Opacity { get; set; }

    /// <summary>
    /// Specifies the size of the symbol, in pixels.
    /// When used with an image file, this specifies the height of the image, with the width being scaled accordingly.
    /// if omitted the native symbol size is used. Value may contain
    /// </summary>
    [XmlElement("Size")]
    public ParameterValue Size { get; set; }

    /// <summary>
    /// Specifies the rotation of the symbol about its center point, in decimal degrees.
    /// Positive values indicate rotation in the clockwise direction, negative values indicate counter-clockwise rotation.
    /// Value may contain expressions. Default is 0.
    /// </summary>
    [XmlElement("Rotation")]
    public ParameterValue Rotation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("Displacement")]
    public Displacement Displacement { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("AnchorPoint")]
    public AnchorPoint AnchorPoint { get; set; }

    public void Accept(IStyleVisitor visitor, object extraData)
    {
    }
}
using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
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
        public float Opacity { get; set; }

        /// <summary>
        /// Specifies the size of the symbol, in pixels.
        /// When used with an image file, this specifies the height of the image, with the width being scaled accordingly.
        /// if omitted the native symbol size is used. Value may contain
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Specifies the rotation of the symbol about its center point, in decimal degrees.
        /// Positive values indicate rotation in the clockwise direction, negative values indicate counter-clockwise rotation.
        /// Value may contain expressions. Default is 0.
        /// </summary>
        public float Rotation { get; set; }
        public Graphic()
        {
            GraphicalSymbols = new List<GraphicalSymbol>();
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Graphic" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "Opacity" when reader.NodeType == XmlNodeType.Element:
                            Opacity = reader.ReadContentAsFloat();
                            break;
                        case "Size" when reader.NodeType == XmlNodeType.Element:
                            Size = reader.ReadContentAsInt();
                            break;
                        case "Rotation" when reader.NodeType == XmlNodeType.Element:
                            Rotation = reader.ReadContentAsFloat();
                            break;
                        case "ExternalGraphic" when reader.NodeType == XmlNodeType.Element:
                            var graphicalSymbol = new ExternalGraphic();
                            graphicalSymbol.ReadXml(reader);
                            GraphicalSymbols.Add(graphicalSymbol);
                            break;
                        case "Mark" when reader.NodeType == XmlNodeType.Element:
                            var mark = new Mark();
                            mark.ReadXml(reader);
                            GraphicalSymbols.Add(mark);
                            break;
                    }
            }
        }
    }
}
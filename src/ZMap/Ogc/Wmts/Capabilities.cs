namespace ZMap.Ogc.Wmts;

[XmlRoot(ElementName = "Capabilities")]
public class Capabilities
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement(ElementName = "Contents")]
    public ContentsMeta Contents { get; set; }

    public static Capabilities LoadFromXml(string xml)
    {
        var serializer = new XmlSerializer(typeof(Capabilities));
        using var reader = new NamespaceIgnorantXmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
        var capabilities = (Capabilities)serializer.Deserialize(reader);
        return capabilities;
    }

    public class ContentsMeta
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "TileMatrixSet")]
        public TileMatrixSet TileMatrixSet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Layer")]
        public Layer Layer { get; set; }
    }

    public class Layer
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string Identifier { get; set; }
        public string Format { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        public string LowerCorner { get; set; }
        public string UpperCorner { get; set; }
    }

    public class TileMatrixSet
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "SupportedCRS")]
        public string SupportedCRS { get; set; }

        [XmlElement(ElementName = "TileMatrix")]
        public TileMatrix[] TileMatrices { get; set; }
    }

    public class TileMatrix
    {
        [XmlElement(ElementName = "Identifier")]
        public string Identifier { get; set; }

        [XmlElement(ElementName = "ScaleDenominator")]
        public double ScaleDenominator { get; set; }

        [XmlElement(ElementName = "TopLeftCorner")]
        public string TopLeftCorner { get; set; }

        [XmlElement(ElementName = "TileWidth")]
        public int TileWidth { get; set; }

        [XmlElement(ElementName = "TileHeight")]
        public int TileHeight { get; set; }

        [XmlElement(ElementName = "MatrixWidth")]
        public int MatrixWidth { get; set; }

        [XmlElement(ElementName = "MatrixHeight")]
        public int MatrixHeight { get; set; }
    }
}
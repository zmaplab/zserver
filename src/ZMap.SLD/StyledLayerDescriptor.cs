using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ZMap.SLD
{
    /// <summary>
    /// https://schemas.opengis.net/sld/1.1/StyledLayerDescriptor.xsd
    /// </summary>
    [XmlRoot(ElementName = "StyledLayerDescriptor")]
    public class StyledLayerDescriptor
    {
        /// <summary>
        /// Required: false
        /// 名称
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Description")]
        public Description Description { get; set; }

        /// <summary>
        /// Required: true
        /// 版本
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        /// 对服务器目录中命名层的引用
        /// NamedLayer 与 UserLayer 只能有一种
        /// </summary>
        [XmlElement(ElementName = "NamedLayer", Type = typeof(NamedLayer))]
        [XmlElement(ElementName = "UserLayer", Type = typeof(UserLayer))]
        public List<StyledLayer> Layers { get; set; }

        /// <summary>
        /// Required: false
        /// The UseSLDLibrary tag specifies that an external SLD document
        /// should be used as a "library" of named layers and styles to
        /// augment the set of named layers and styles that are available
        /// for use inside of a WMS.  In the event of name collisions, the
        ///     SLD library takes precedence over the ones internal to the WMS.
        ///     Any number of libraries may be specified in an SLD and each
        /// successive library takes precedence over the former ones in the
        /// case of name collisions.
        /// </summary>
        [XmlElement("OnlineResource")]
        public List<OnlineResource> UseSldLibraries { get; set; }

        public void Valid()
        {
            // TODO: 校验 NamedLayer 和 UserLayer 是否只有一种
        }

        public void Accept(IStyleVisitor visitor, object data)
        {
            visitor.Visit(this, data);
        }

        public static StyledLayerDescriptor Load(string file)
        {
            var serializer = new XmlSerializer(typeof(StyledLayerDescriptor));
            var reader = new NamespaceIgnorantXmlTextReader(File.OpenRead(file));
            return serializer.Deserialize(reader) as
                StyledLayerDescriptor;
        }

        public static StyledLayerDescriptor Load(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(StyledLayerDescriptor));
            var reader = new NamespaceIgnorantXmlTextReader(stream);
            return serializer.Deserialize(reader) as
                StyledLayerDescriptor;
        }

        // public override bool Equals(object obj)
        // {
        //     if (this == obj)
        //     {
        //         return true;
        //     }
        //
        //     if (obj is StyledLayerDescriptor other)
        //     {
        //         return (string.Equals(Abstract, other.Abstract)
        //                 && NamedLayers == other.NamedLayers
        //                 && string.Equals(Name, other.Name)
        //                 && string.Equals(Title, other.Title));
        //     }
        //
        //     return false;
        // }

        // public override int GetHashCode()
        // {
        //     return Objects.hash(name, title, abstractStr, layers);
        // }
    }
}
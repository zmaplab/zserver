using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class NamespaceIgnorantXmlTextReader : XmlTextReader
    {
        public NamespaceIgnorantXmlTextReader(Stream stream) : base(stream)
        {
        }
    
        public override string NamespaceURI => "";
    }

    [XmlRoot(ElementName = "StyledLayerDescriptor")]
    public class StyledLayerDescriptor
    {
        // public const string SE = "http://www.opengis.net/se";
        // public const string SLD = "http://www.opengis.net/sld";

        /// <summary>
        /// Required: false
        /// 名称
        /// </summary>
        [XmlElement("Name" )]
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

        public void Accept(IStyleVisitor visitor)
        {
        }

        public static StyledLayerDescriptor Load(string path)
        {
            var serializer = new XmlSerializer(typeof(StyledLayerDescriptor));

            // var rdr = XmlReader.Create(stream, new XmlReaderSettings
            // {
            //     
            //     XmlResolver = new XmlSecureResolver(new XmlUrlResolver(),
            //         new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None))
            // });
            // rdr.Namespaces = false;

            // var namespaces = new XmlSerializerNamespaces();
            // namespaces.Add("ac", "http://www.example.org/Standards/xyz/1");
            // namespaces.Add("rlc", "http://www.example.org/Standards/def/1");
            // namespaces.Add("def1", "http://www.lol.com/Standards/lol.xsd");
            var reader = new NamespaceIgnorantXmlTextReader(File.OpenRead(path));
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


        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.EOF || (reader.NodeType == XmlNodeType.EndElement && reader.Name == "StyledLayerDescriptor"))
        //         {
        //             break;
        //         }
        //         else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "NamedLayer")
        //         {
        //             var namedLayer = new NamedLayer();
        //             namedLayer.ReadXml(reader);
        //             NamedLayers.Add(namedLayer);
        //         }
        //         else if (reader.LocalName == "StyledLayerDescriptor" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //     }
        // }
    }
}
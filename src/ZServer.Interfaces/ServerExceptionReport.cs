using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

[assembly: InternalsVisibleTo("ZServer.Tests")]

namespace ZServer.Interfaces;

[XmlRoot("ServerExceptionReport")]
public class ServerExceptionReport
{
    private static readonly XmlSerializer Serializer = new(typeof(ServerExceptionReport));

    private static readonly XmlWriterSettings XmlWriterSettings = new()
    {
        Encoding = new UTF8Encoding(false),
        Indent = true
    };

    private static readonly XmlSerializerNamespaces Namespaces = new(
        new[]
        {
            new XmlQualifiedName("xsd", "http://schemas.opengis.net/wms/1.1.1/OGC-exception.xsd")
        });

    /// <summary>
    /// 
    /// </summary>
    [XmlElement("ServerException")]
    public List<ServerException> Exceptions { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("version")]
    public string Version { get; set; } = "1.1.1";

    public byte[] Serialize(string format = "text/xml")
    {
        switch (format)
        {
            case "json":
            {
                return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
            }
            default:
            {
                using var mem = new MemoryStream();
                using var writer = XmlWriter.Create(mem, XmlWriterSettings);
                Serializer.Serialize(writer, this, Namespaces);
                writer.Flush();
                return mem.ToArray();
            }
        }
    }
}
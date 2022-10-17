using System.IO;
using System.Xml;

namespace ZMap.SLD;

public class NamespaceIgnorantXmlTextReader : XmlTextReader
{
    public NamespaceIgnorantXmlTextReader(Stream stream) : base(stream)
    {
    }

    public override string NamespaceURI => "";
}
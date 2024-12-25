namespace ZMap.Infrastructure;

public class NamespaceIgnorantXmlTextReader(Stream stream) : XmlTextReader(stream)
{
    public override string NamespaceURI => "";
}
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ZMap.SLD.Expression;

public class Text : Expression, IXmlSerializable
{
    public string Value { get; set; }

    public override object Accept(IExpressionVisitor visitor, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public override object Evaluate(object @object)
    {
        throw new System.NotImplementedException();
    }

    public override T Evaluate<T>(object @object)
    {
        throw new System.NotImplementedException();
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        reader.ReadStartElement();
        Value = reader.ReadString();
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        throw new System.NotImplementedException();
    }
}
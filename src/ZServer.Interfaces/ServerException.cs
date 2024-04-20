using System.Xml.Serialization;
using Orleans;

namespace ZServer.Interfaces;

[XmlRoot("ServerException")]
[GenerateSerializer]
[Immutable]
public class ServerException
{
    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("code")]
    [Id(0)]
    public string Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlAttribute("locator")]
    [Id(1)]
    public string Locator { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [XmlText]
    [Id(2)]
    public string Text { get; set; }
}
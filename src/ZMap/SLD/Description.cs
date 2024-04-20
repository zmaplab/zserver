using System.Xml.Serialization;

namespace ZMap.SLD;

/// <summary>
///  A "Description" gives human-readable descriptive information for
///  the object it is included within.
/// </summary>
public class Description
{
    /// <summary>
    /// Required: false
    /// 样式的标题
    /// </summary>
    [XmlElement("Title")]
    public string Title { get; set; }

    /// <summary>
    /// Required: false
    /// 样式的说明
    /// </summary>
    [XmlElement("Abstract")]
    public string Abstract { get; set; }

    // public void ReadXml(XmlReader reader)
    // {
    //     while (reader.Read())
    //     {
    //         if (reader.LocalName == "Description" && reader.NodeType == XmlNodeType.EndElement)
    //         {
    //             break;
    //         }
    //         else
    //             switch (reader.LocalName)
    //             {
    //                 case "Title" when reader.NodeType == XmlNodeType.Element:
    //                     Title = reader.ReadString();
    //                     break;
    //                 case "Abstract" when reader.NodeType == XmlNodeType.Element:
    //                     Abstract = reader.ReadString();
    //                     break;
    //             }
    //     }
    // }
}
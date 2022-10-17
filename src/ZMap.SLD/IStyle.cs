// using System.Collections.Generic;
// using System.Xml;
// using System.Xml.Serialization;
//
// namespace ZMap.SLD
// {
//     public interface IStyle
//     {
//         /// <summary>
//         /// 名称
//         /// </summary>
//         string Name { get; set; }
//
//         /// <summary>
//         /// 描述
//         /// </summary>
//         Description Description { get; set; }
//
//         /// <summary>
//         /// 
//         /// </summary>
//         bool IsDefault { get; set; }
//
//         ISymbolizer DefaultSpecification { get; set; }
//
//         /// <summary>
//         /// 
//         /// </summary>
//         // public List< FeatureTypeStyle> FeatureTypeStyles { get; set; }
//
//         // public List<Rule> Rules { get; set; }
//         //
//         // public Style()
//         // {
//         //     Rules = new List<Rule>();
//         // }
//         abstract object Accept(IStyleVisitor visitor, object extraData);
//
//         // public void ReadXml(XmlReader reader)
//         // {
//         //     while (reader.Read())
//         //     {
//         //         if (reader.LocalName == "Style" && reader.NodeType == XmlNodeType.EndElement)
//         //         {
//         //             break;
//         //         }
//         //         else if (reader.LocalName == "Rule" && reader.NodeType == XmlNodeType.Element)
//         //         {
//         //             var rule = new Rule();
//         //             rule.ReadXml(reader);
//         //             Rules.Add(rule);
//         //         }
//         //     }
//         // }
//     }
// }
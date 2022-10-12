// using System.Collections.Generic;
// using System.Xml;
//
// namespace ZMap.SLD
// {
//     public class NamedLayer
//     {
//         public string Name { get; set; }
//         public List<UserStyle> UserStyles { get; set; }
//
//         public NamedLayer()
//         {
//             UserStyles = new List<UserStyle>();
//         }
//
//         public void ReadXml(XmlReader reader)
//         {
//             while (reader.Read())
//             {
//                 if (reader.LocalName == "NamedLayer" && reader.NodeType == XmlNodeType.EndElement)
//                 {
//                     break;
//                 }
//                 else
//                     switch (reader.LocalName)
//                     {
//                         case "Name" when reader.NodeType == XmlNodeType.Element:
//                             Name = reader.ReadString();
//                             break;
//                         case "UserStyle" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var userStyle = new UserStyle();
//                                 userStyle.ReadXml(reader);
//                                 UserStyles.Add(userStyle);
//                                 break;
//                             }
//                     }
//
//             }
//         }
//     }
// }
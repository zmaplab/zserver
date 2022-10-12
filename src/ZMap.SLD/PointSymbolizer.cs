// using System.Xml;
//
// namespace ZMap.SLD
// {
//     public class PointSymbolizer : ISymbolizer
//     {
//         /// <summary>
//         /// 图像文件的位置。
//         /// </summary>
//         public OnlineResource OnlineResource { get; set; }
//         /// <summary>
//         /// 几何体属性
//         /// </summary>
//         public Geometry Geometry { get; set; }
//         /// <summary>
//         /// 指定点符号的样式
//         /// </summary>
//         public Graphic Graphic { get; set; }
//
//         public void ReadXml(XmlReader reader)
//         {
//             while (reader.Read())
//             {
//                 if (reader.LocalName == "PointSymbolizer" && reader.NodeType == XmlNodeType.EndElement)
//                 {
//                     break;
//                 }
//                 else
//                     switch (reader.LocalName)
//                     {
//                         case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var onlineResource = new OnlineResource();
//                                 onlineResource.ReadXml(reader);
//                                 OnlineResource = onlineResource;
//                                 break;
//                             }
//                         case "Geometry" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var geometry = new Geometry();
//                                 geometry.ReadXml(reader);
//                                 Geometry = geometry;
//                                 break;
//                             }
//                         case "Graphic" when reader.NodeType == XmlNodeType.Element:
//                             {
//                                 var graphic = new Graphic();
//                                 graphic.ReadXml(reader);
//                                 Graphic = graphic;
//                                 break;
//                             }
//                     }
//             }
//         }
//     }
// }
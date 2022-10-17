using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
 

namespace ZMap.SLD
{
    public class Fill
    {
        /// <summary>
        /// 填充时是否反锯齿（可选，默认值为 true）
        /// </summary>
        public bool Antialias { get; set; } = true;

        /// <summary>
        /// 填充的不透明度（可选，取值范围为 0 ~ 1，默认值为 1）
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// 填充的颜色（可选，默认值为 #000000。如果设置了 Graphic， 则 color 将无效）
        /// </summary>
        public string Color { get; set; } = "000000";

        /// <summary>
        /// 填充的平移（可选，通过平移 [x, y] 达到一定的偏移量。默认值为 [0, 0]，单位：像素。）
        /// </summary>
        public int[] Translate { get; set; } = new int[2] { 0, 0 };

        // /// <summary>
        // /// 平移的锚点，即相对的参考物（可选，可选值为 map、viewport，默认为 map）
        // /// </summary>
        // public TranslateAnchor TranslateAnchor { get; set; } = TranslateAnchor.Map;

        /// <summary>
        /// TODO: 用于填充图案, 可以是线上图片
        /// </summary>
        public GraphicFill GraphicFill { get; set; }

        // public void ReadXml(XmlReader reader)
        // {
        //     var translate = new List<int>();
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "Fill" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else if (reader.LocalName == "SvgParameter" && reader.NodeType == XmlNodeType.Element)
        //         {
        //             var attribute = reader.GetAttribute("name").ToLower();
        //             switch (attribute)
        //             {
        //                 case "fill":
        //                     {
        //                         Color = reader.ReadElementContentAsString();
        //                         break;
        //                     }
        //                 case "fill-antialias":
        //                     {
        //                         Antialias = reader.ReadElementContentAsBoolean();
        //                         break;
        //                     }
        //                 case "fill-opacity":
        //                     {
        //                         Opacity = reader.ReadElementContentAsFloat();
        //                         break;
        //                     }
        //                 case "fill-translate":
        //                     {
        //                         var val = reader.ReadElementContentAsInt();
        //                         translate.Add(val);
        //                         break;
        //                     }
        //                 case "fill-translateanchor" when reader.NodeType == XmlNodeType.Element:
        //                     {
        //                         var val = reader.ReadElementContentAsString();
        //                         if (Enum.TryParse<TranslateAnchor>(val, false, out var translateAnchor))
        //                         {
        //                             TranslateAnchor = translateAnchor;
        //                         }
        //                         break;
        //                     }
        //             }
        //         }
        //         else if (reader.LocalName == "GraphicFill" && reader.NodeType == XmlNodeType.Element)
        //         {
        //             var graphicFill = new GraphicFill();
        //             graphicFill.ReadXml(reader);
        //             GraphicFill = graphicFill;
        //         }
        //     }
        //     if (translate.Any())
        //         Translate = translate.ToArray();
        // }
    }
}
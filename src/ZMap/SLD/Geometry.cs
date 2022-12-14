namespace ZMap.SLD
{
    public class Geometry
    {
        /// <summary>
        /// 图形所在列
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 图形转换方法名
        /// </summary>
        public string Function { get; set; }

        public string[] Literal { get; set; }


        // public void ReadXml(XmlReader reader)
        // {
        //     var literal = new List<string>();
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "Geometry" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else
        //             switch (reader.LocalName)
        //             {
        //                 case "PropertyName" when reader.NodeType == XmlNodeType.Element:
        //                     PropertyName = reader.ReadString();
        //                     break;
        //                 case "Function" when reader.NodeType == XmlNodeType.Element:
        //                     Function = reader.ReadString();
        //                     break;
        //                 case "Literal" when reader.NodeType == XmlNodeType.Element:
        //                     var val = reader.ReadString();
        //                     literal.Add(val);
        //                     break;
        //             }
        //     }
        //
        //     Literal = literal.ToArray();
        // }
    }
}
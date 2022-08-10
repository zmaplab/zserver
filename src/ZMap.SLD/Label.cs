using System.Xml;

namespace ZMap.SLD
{
    public class Label
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 先使用 c# 表达式
        /// </summary>
        public string EvalExpression { get; set; }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Label" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "PropertyName" when reader.NodeType == XmlNodeType.Element:
                            PropertyName = reader.ReadString();
                            break;
                        case "EvalExpression" when reader.NodeType == XmlNodeType.Element:
                            EvalExpression = reader.ReadString();
                            break;
                    }
            }
        }
    }
}
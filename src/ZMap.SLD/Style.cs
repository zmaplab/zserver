using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public class Style
    {
        public List<Rule> Rules { get; set; }
        public Style()
        {
            Rules=new List<Rule>();
        }
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Style" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else if (reader.LocalName == "Rule" && reader.NodeType == XmlNodeType.Element)
                {
                    var rule=new Rule();
                    rule.ReadXml(reader);
                    Rules.Add(rule);
                }
            }
        }
    }
}
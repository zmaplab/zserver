using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public abstract class Style
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public Description Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // public List< FeatureTypeStyle> FeatureTypeStyles { get; set; }
        
        // public List<Rule> Rules { get; set; }
        //
        // public Style()
        // {
        //     Rules = new List<Rule>();
        // }

        public abstract object Accept(IStyleVisitor visitor, object extraData);

        // public void ReadXml(XmlReader reader)
        // {
        //     while (reader.Read())
        //     {
        //         if (reader.LocalName == "Style" && reader.NodeType == XmlNodeType.EndElement)
        //         {
        //             break;
        //         }
        //         else if (reader.LocalName == "Rule" && reader.NodeType == XmlNodeType.Element)
        //         {
        //             var rule = new Rule();
        //             rule.ReadXml(reader);
        //             Rules.Add(rule);
        //         }
        //     }
        // }
    }
}
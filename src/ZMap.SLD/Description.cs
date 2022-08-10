using System.Xml;

namespace ZMap.SLD
{
    public class Description
    {        
        public string Title { get; private set; }
        public string Abstract { get; private set; }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "Description" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "Title" when reader.NodeType == XmlNodeType.Element:
                            Title = reader.ReadString();
                            break;
                        case "Abstract" when reader.NodeType == XmlNodeType.Element:
                            Abstract = reader.ReadString();
                            break;
                    }
            }
        }
    }
}
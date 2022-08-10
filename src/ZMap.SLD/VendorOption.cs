using System.Xml;

namespace ZMap.SLD
{
    public class VendorOption
    {
        public string Distance { get; set; }
        public void ReadXml(XmlReader reader)
        {
            var attribute = reader.GetAttribute("name").ToLower();
            switch (attribute)
            {
                case "distance":
                    {
                        Distance = reader.ReadElementContentAsString();
                        break;
                    }
            }
        }
    }
}
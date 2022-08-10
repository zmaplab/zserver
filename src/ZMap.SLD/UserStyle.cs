using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public class UserStyle
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<FeatureTypeStyle> FeatureTypeStyles { get; set; }
        public List<OnlineResource> OnlineResources { get; set; }
        public List<CoverageStyle> CoverageStyles { get; set; }
        public UserStyle()
        {
            FeatureTypeStyles = new List<FeatureTypeStyle>();
            OnlineResources = new List<OnlineResource>();
            CoverageStyles = new List<CoverageStyle>();
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.LocalName == "UserStyle" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                else
                    switch (reader.LocalName)
                    {
                        case "Name" when reader.NodeType == XmlNodeType.Element:
                            Name = reader.ReadString();
                            break;
                        case "IsDefault" when reader.NodeType == XmlNodeType.Element:
                            IsDefault = reader.ReadElementContentAsBoolean();
                            break;
                        case "FeatureTypeStyle" when reader.NodeType == XmlNodeType.Element:
                            {
                                var featureTypeStyle = new FeatureTypeStyle();
                                featureTypeStyle.ReadXml(reader);
                                FeatureTypeStyles.Add(featureTypeStyle);
                                break;
                            }
                        case "OnlineResource" when reader.NodeType == XmlNodeType.Element:
                            {
                                var onlineResource = new OnlineResource();
                                onlineResource.ReadXml(reader);
                                OnlineResources.Add(onlineResource);
                                break;
                            }
                        case "CoverageStyle" when reader.NodeType == XmlNodeType.Element:
                            {
                                var coverageStyle = new CoverageStyle();
                                coverageStyle.ReadXml(reader);
                                CoverageStyles.Add(coverageStyle);
                                break;
                            }
                    }
            }
        }
    }
}
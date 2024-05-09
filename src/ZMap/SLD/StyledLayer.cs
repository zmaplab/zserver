namespace ZMap.SLD;

[XmlInclude(typeof(NamedLayer))]
[XmlInclude(typeof(UserLayer))]
public class StyledLayer
{
    [XmlElement(ElementName = "Name")]
    public string Name { get; set; }

    [XmlElement(ElementName = "Description")]
    public Description Description { get; set; }
}
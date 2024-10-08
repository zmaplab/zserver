namespace ZMap.SLD;

/// <summary>
/// A UserStyle allows user-defined styling and is semantically
/// equivalent to a WMS named style. External FeatureTypeStyles or
/// CoverageStyles can be linked using an OnlineResource-element
/// </summary>
public class UserStyle : Style
{
    /// <summary>
    /// 
    /// </summary>
    [XmlElement("IsDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 1..N
    /// 定义用于呈现单个要素类型的符号。
    /// </summary>
    [XmlElement("FeatureTypeStyle")]
    public List<FeatureTypeStyle> FeatureTypeStyles { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<OnlineResource> OnlineResources { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<CoverageStyle> CoverageStyles { get; set; }

    public void Valid()
    {
        // TODO: 校验 FeatureTypeStyles、CoverageStyles、 OnlineResources 只能有一种
    }

    public UserStyle()
    {
        // FeatureTypeStyles = new List<FeatureTypeStyle>();
        // OnlineResources = new List<OnlineResource>();
        // CoverageStyles = new List<CoverageStyle>();
    }

    public override object Accept(IStyleVisitor visitor, object data)
    {
        return visitor.Visit(this, data);
    }
}
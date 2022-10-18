namespace ZMap.SLD;

/// <summary>
/// A NamedStyle is used to refer to a style that has a name in a WMS.
/// </summary>
public class NamedStyle : Style
{
    public override void Accept(IStyleVisitor visitor, object data)
    {
        visitor.Visit(this, data);
    }
}
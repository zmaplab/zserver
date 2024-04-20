namespace ZMap.SLD;

public interface IStyle
{
    object Accept(IStyleVisitor visitor, object extraData);
}
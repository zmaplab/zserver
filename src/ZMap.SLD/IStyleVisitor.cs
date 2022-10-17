namespace ZMap.SLD;

public interface IStyleVisitor
{
    void Visit(StyledLayerDescriptor sld);
}
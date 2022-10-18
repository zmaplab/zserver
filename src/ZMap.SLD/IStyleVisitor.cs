namespace ZMap.SLD;

public interface IStyleVisitor
{
    object Visit(StyledLayerDescriptor sld, object data);
    object Visit(Style style, object data);
    object Visit(NamedStyle style, object data);
    object Visit(UserStyle style, object data);
    object Visit(NamedLayer namedLayer, object data);
    object Visit(UserLayer userLayer, object data);
    object Visit(FeatureTypeConstraint featureTypeConstraint, object data);
    object Visit(FeatureTypeStyle featureTypeStyle, object data);
    object Visit(Rule rule, object data);
    object Visit(Symbolizer symbolizer, object data);
}
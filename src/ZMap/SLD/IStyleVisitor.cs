using System.Collections.Generic;
using ZMap.SLD.Expression;
using ZMap.SLD.Filter;

namespace ZMap.SLD;

public interface IStyleVisitor
{
    void Push(dynamic obj);
    dynamic Pop();
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
    object Visit(PolygonSymbolizer polygonSymbolizer, object data);
    object Visit(LineSymbolizer lineSymbolizer, object data);
    object Visit(Fill fill, object data);
    object Visit(Stroke stroke, object data);
    object Visit(Graphic graphic, object data);
    object Visit(Halo halo, object data);
    object Visit(LinePlacement linePlacement, object data);
    object Visit(Mark mark, object data);
    object Visit(PointPlacement pointPlacement, object data);
    object Visit(ParameterValue parameterValue, object data);
    object Visit(Literal literal, object data);
    object Visit(PropertyIsEqualTo propertyIsEqualTo, object data);
    object Visit(Expression.Expression expression, object data);

}
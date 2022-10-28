using System.Text;
using ZMap.SLD.Expression;
using ZMap.SLD.Filter;

namespace ZMap.SLD;

public class ZMapStyleVisitor : IStyleVisitor, IFilterVisitor, IExpressionVisitor
{
    private readonly StringBuilder _stringBuilder;

    public ZMapStyleVisitor()
    {
        _stringBuilder = new StringBuilder();
    }

    public object Visit(StyledLayerDescriptor sld, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Style style, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(NamedStyle style, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(UserStyle style, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(NamedLayer namedLayer, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(UserLayer userLayer, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(FeatureTypeConstraint featureTypeConstraint, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(FeatureTypeStyle featureTypeStyle, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Rule rule, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Symbolizer symbolizer, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(PropertyIsEqualTo filter, object data)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(NilExpression expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Add expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Div expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Function expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Literal expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Mul expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(PropertyName expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Sub expression, object extraData)
    {
        throw new System.NotImplementedException();
    }
}
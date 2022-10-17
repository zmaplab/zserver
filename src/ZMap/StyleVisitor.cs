using ZMap.SLD;

namespace ZMap;

public class StyleVisitor : IStyleVisitor
{
    public object Visit(StyledLayerDescriptor sld, object data)
    {
        foreach (var styledLayer in sld.Layers)
        {
            if (styledLayer is NamedLayer namedLayer)
            {
                namedLayer.Accept(this, data);
            }
            else if (styledLayer is UserLayer userLayer)
            {
                userLayer.Accept(this, data);
            }
        }

        return null;
    }

    public object Visit(SLD.Style style, object data)
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

    public object Visit(Description description, object data)
    {
        throw new System.NotImplementedException();
    }
}
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
        style.Accept(this, data);
        return null;
    }

    public object Visit(NamedStyle style, object data)
    {
        return null;
    }

    public object Visit(UserStyle style, object data)
    {
        foreach (var featureTypeStyle in  style.FeatureTypeStyles)
        {
            featureTypeStyle.Accept(this, data);
        }

        return null;
    }

    public object Visit(NamedLayer namedLayer, object data)
    {
        foreach (var style in namedLayer.Styles)
        {
            style.Accept(this, data);
        }

        foreach (FeatureTypeConstraint ftc in namedLayer.FeatureConstraints)
        {
            ftc.Accept(this, data);
        }

        return null;
    }

    public object Visit(UserLayer userLayer, object data)
    {
        foreach (var style in userLayer.Styles)
        {
            style.Accept(this, data);
        }

        foreach (FeatureTypeConstraint ftc in userLayer.FeatureConstraints)
        {
            ftc.Accept(this, data);
        }

        return null;
    }

    public object Visit(FeatureTypeConstraint featureTypeConstraint, object data)
    {
        return null;
    }

    public object Visit(FeatureTypeStyle featureTypeStyle, object data)
    {
        foreach (var rule in featureTypeStyle.Rules)
        {
            rule.Accept(this, data);
        }

        return null;
    }

    public object Visit(Rule rule, object data)
    {
        foreach (var symbolizer in rule.Symbolizers)
        {
            symbolizer.Accept(this, data);
        }

        return null;
    }

    public object Visit(Symbolizer symbolizer, object data)
    {
        throw new System.NotImplementedException();
    }
}
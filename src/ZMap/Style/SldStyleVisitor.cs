using System;
using System.Collections.Generic;
using ZMap.SLD;
using ZMap.SLD.Filter;
using ZMap.SLD.Filter.Expression;

namespace ZMap.Style;

public class SldStyleVisitor : IStyleVisitor, IFilterVisitor
{
    private readonly Stack<dynamic> _stack;

    public List<StyleGroup> StyleGroups { get; }

    public SldStyleVisitor()
    {
        _stack = new Stack<dynamic>();
        StyleGroups = new List<StyleGroup>();
    }

    public void Push(dynamic obj)
    {
        _stack.Push(obj);
    }

    public dynamic Pop()
    {
        return _stack.Pop();
    }

    public object Visit(StyledLayerDescriptor sld, object data)
    {
        foreach (var styledLayer in sld.Layers)
        {
            switch (styledLayer)
            {
                case NamedLayer namedLayer:
                    namedLayer.Accept(this, data);
                    break;
                case UserLayer userLayer:
                    userLayer.Accept(this, data);
                    break;
            }
        }

        return new List<StyleGroup>();
    }

    public object Visit(SLD.Style style, object data)
    {
        style.Accept(this, data);
        return null;
    }

    public object Visit(NamedStyle style, object data)
    {
        style.Accept(this, data);
        return null;
    }

    public object Visit(UserStyle style, object data)
    {
        foreach (var featureTypeStyle in style.FeatureTypeStyles)
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

        foreach (var ftc in namedLayer.FeatureConstraints)
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

        foreach (var ftc in userLayer.FeatureConstraints)
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
        var styleGroup = new StyleGroup
        {
            MinZoom = rule.MinScaleDenominator,
            MaxZoom = rule.MaxScaleDenominator,
            ZoomUnit = ZoomUnits.Scale,
            Name = rule.Name,
            Description = rule.Description?.Title,
            Filter = CSharpExpression<bool?>.New(null)
        };

        if (rule.FilterType != null)
        {
            rule.FilterType.Accept(this, data);
            if (_stack.Pop() is CSharpExpression filterExpression)
            {
                styleGroup.Filter = CSharpExpression<bool?>.New(null,
                    $"{(rule.ElseFilter ? "!" : string.Empty)}{filterExpression.Body}");
            }
        }

        foreach (var symbolizer in rule.Symbolizers)
        {
            symbolizer.Accept(this, data);
        }

        foreach (var el in _stack)
        {
            if (el is Style style)
            {
                styleGroup.Styles.Add(style);
            }
        }

        styleGroup.Styles.Reverse();

        StyleGroups.Add(styleGroup);
        return null;
    }

    public object Visit(Symbolizer symbolizer, object data)
    {
        symbolizer.Accept(this, data);
        return null;
    }

    public object Visit(PolygonSymbolizer polygonSymbolizer, object data)
    {
        return null;
    }

    public object Visit(LineSymbolizer lineSymbolizer, object data)
    {
        lineSymbolizer.Accept(this, data);
        return null;
    }

    public object Visit(Fill fill, object data)
    {
        fill.Accept(this, data);
        return null;
    }

    public object Visit(Stroke stroke, object data)
    {
        stroke.Accept(this, data);
        return null;
    }

    public object Visit(Graphic graphic, object data)
    {
        graphic.Accept(this, data);
        return null;
    }

    public object Visit(Halo halo, object data)
    {
        halo.Accept(this, data);
        return null;
    }

    public object Visit(LinePlacement linePlacement, object data)
    {
        throw new NotImplementedException();
    }

    public object Visit(Mark mark, object data)
    {
        throw new NotImplementedException();
    }

    public object Visit(PointPlacement pointPlacement, object data)
    {
        throw new NotImplementedException();
    }

    public object Visit(ParameterValue parameterValue, object data)
    {
        return parameterValue?.Accept(this, data);
    }

    public object Visit(NilExpression expression, object extraData)
    {
        return null;
    }

    public object Visit(Add expression, object extraData)
    {
        expression.Accept(this, extraData);
        return null;
    }

    public object Visit(Div expression, object extraData)
    {
        expression.Accept(this, extraData);
        return null;
    }

    public object Visit(FunctionType1 expression, object extraData)
    {
        throw new NotImplementedException();
    }

    // public object Visit(Function expression, object extraData)
    // {
    //     expression.Accept(this, expression);
    //     return null;
    // }

    public object Visit(LiteralType literal, object data)
    {
        literal.Accept(this, data);
        return null;
    }

    public object Visit(Mul expression, object extraData)
    {
        expression.Accept(this, extraData);
        return null;
    }

    public object Visit(PropertyNameType expression, object extraData)
    {
        expression.Accept(this, expression);
        return null;
    }

    public object Visit(Sub expression, object extraData)
    {
        expression.Accept(this, extraData);
        return null;
    }

    public object Visit(PropertyIsEqualToType filter, object data)
    {
        filter.Accept(this, data);
        return null;
    }

    public object Visit(LogicOpsType logicOpsType, object data)
    {
        logicOpsType.Accept(this, data);
        return null;
    }

    public object Visit(ComparisonOpsType comparisonOpsType, object data)
    {
        comparisonOpsType.Accept(this, data);
        return null;
    }

    public object Visit(UpperBoundaryType upperBoundaryType, object data)
    {
        upperBoundaryType.Accept(this, data);
        return null;
    }

    public object Visit(And and, object data)
    {
        and.Accept(this, data);
        return null;
    }

    public object Visit(PropertyIsGreaterThanOrEqualTo filter, object data)
    {
        filter.Accept(this, data);
        return null;
    }

    public object Visit(PropertyIsLessThan filter, object data)
    {
        filter.Accept(this, data);
        return null;
    }

    public object VisitObject(object obj, object extraData)
    {
        switch (obj)
        {
            case LogicOpsType logicOpsType:
                Visit(logicOpsType, extraData);
                break;
            case ComparisonOpsType comparisonOpsType:
                Visit(comparisonOpsType, extraData);
                break;
            case UpperBoundaryType upperBoundaryType:
                Visit(upperBoundaryType, extraData);
                break;
            case ExpressionType expressionType:
                Visit(expressionType, extraData);
                break;
            default:
                throw new NotImplementedException();
        }

        return null;
    }

    public object Visit(ExpressionType expression, object data)
    {
        expression.Accept(this, data);
        return null;
    }
}
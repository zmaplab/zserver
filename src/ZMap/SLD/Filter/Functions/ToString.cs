using System;
using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter.Functions;

public class ToString
{
    private readonly FunctionType1 _functionType1;

    public ToString(FunctionType1 functionType1)
    {
        _functionType1 = functionType1;
    }

    public object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Visit(_functionType1.Items[0], extraData);

        var expression = (ZMap.Style.CSharpExpression)visitor.Pop();
        var guid = Guid.NewGuid().ToString("N");

        var resultExpression = ZMap.Style.CSharpExpression.New($$"""
((Func<string>)(() =>
{
    var value_{{guid}} = {{expression.Expression}};
    return value_{{guid}} == null ? null : value_{{guid}}.ToString();
})).Invoke()
""");
        visitor.Push(resultExpression);

        return null;
    }
}
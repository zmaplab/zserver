using System;
using ZMap.SLD.Filter.Expression;
using ZMap.Utilities;

namespace ZMap.SLD.Filter.Functions;

public class ToArray
{
    private readonly FunctionType1 _functionType1;

    public ToArray(FunctionType1 functionType1)
    {
        _functionType1 = functionType1;
    }

    public object Accept(IExpressionVisitor visitor, object extraData)
    {
        var left = _functionType1.Items[0];
        visitor.Visit(left, extraData);
        var leftExpression = (ZMap.Style.Expression)visitor.Pop();

        // 必需是 LiteralType
        if (_functionType1.Items[1] is not LiteralType right)
        {
            throw new ArgumentException("ToArray 函数第二个参数必须是 LiteralType");
        }

        var type = right.Value;
        ZMap.Style.Expression resultExpression;
        if (type is "string" or "String")
        {
            resultExpression = ZMap.Style.Expression.New($$"""
((Func<{{type}}[]>)(() =>
{
    var value = {{leftExpression.Body}};
    if(value == null){
        return default;
    } else {
        var array = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        return array;
    }
})).Invoke()
""");
        }
        else
        {
            resultExpression = ZMap.Style.Expression.New($$"""
((Func<{{type}}[]>)(() =>
{
    var value = {{leftExpression.Body}};
    if(value == null){
        return default;
    } else {
        var array = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        return array.Select(x => ({{type}})Convert.ChangeType(x, typeof({{type}}))).ToArray();
    }
})).Invoke()
""");
        }

        DynamicCompilationUtilities.GetFunc(resultExpression.Body);
        visitor.Push(resultExpression);
        return null;
    }
}
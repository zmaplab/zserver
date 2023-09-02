using ZMap.SLD.Filter.Expression;

namespace ZMap.SLD.Filter.Functions;

public class Env
{
    private readonly FunctionType1 _functionType1;

    public Env(FunctionType1 functionType1)
    {
        _functionType1 = functionType1;
    }

    public object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Visit(_functionType1.Items[0], extraData);

        var expression = (ZMap.Style.CSharpExpression)visitor.Pop();
        var resultExpression = ZMap.Style.CSharpExpression.New($$"""
feature.GetEnvValue({{expression.Expression}})
""");
        visitor.Push(resultExpression);

        return null;
    }
}
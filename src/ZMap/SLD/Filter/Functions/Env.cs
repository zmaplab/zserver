namespace ZMap.SLD.Filter.Functions;

public class Env(FunctionType1 functionType1)
{
    public object Accept(IExpressionVisitor visitor, object extraData)
    {
        visitor.Visit(functionType1.Items[0], extraData);

        var expression = (CSharpExpressionV2)visitor.Pop();
        var resultExpression = CSharpExpressionV2.Create<dynamic>($$"""
                                                                   feature.GetEnvironmentValue({{expression.FuncName}}(feature))
                                                                   """);
        visitor.Push(resultExpression);

        return null;
    }
}
namespace ZMap.SLD.Filter.Expression;

public interface IExpressionVisitor
{
    void Push(dynamic obj);
    dynamic Pop();
    object Visit(ExpressionType expression, object extraData);
    object Visit(NilExpression expression, object extraData);

    object Visit(Add expression, object extraData);

    object Visit(Div expression, object extraData);

    object Visit(FunctionType1 expression, object extraData);

    object Visit(LiteralType expression, object extraData);

    object Visit(Mul expression, object extraData);

    object Visit(PropertyNameType expression, object extraData);

    object Visit(Sub expression, object extraData);
}
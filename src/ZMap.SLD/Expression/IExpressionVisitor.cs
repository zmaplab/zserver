namespace ZMap.SLD.Expression;

public interface IExpressionVisitor
{
    object Visit(NilExpression expression, object extraData);

    object Visit(Add expression, object extraData);

    object Visit(Div expression, object extraData);

    object Visit(Function expression, object extraData);

    object Visit(Literal expression, object extraData);

    object Visit(Mul expression, object extraData);

    object Visit(PropertyName expression, object extraData);

    object Visit(Sub expression, object extraData);
}
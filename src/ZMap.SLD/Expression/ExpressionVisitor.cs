namespace ZMap.SLD.Expression;

public abstract class ExpressionVisitor
{
    public abstract object Visit(NilExpression expression, object extraData);

    public abstract object Visit(Add expression, object extraData);

    public abstract object Visit(Div expression, object extraData);

    public abstract object Visit(Function expression, object extraData);

    public abstract object Visit(Literal expression, object extraData);

    public abstract object Visit(Mul expression, object extraData);

    public abstract object Visit(PropertyName expression, object extraData);

    public abstract object Visit(Sub expression, object extraData);
}
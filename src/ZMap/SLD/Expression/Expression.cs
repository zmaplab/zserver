using System;

namespace ZMap.SLD.Expression;

public abstract class Expression
{
    public virtual object Accept(IExpressionVisitor visitor, object extraData)
    {
        throw new NotImplementedException();
    }

    public virtual object Evaluate(object @object)
    {
        throw new NotImplementedException();
    }

    public virtual T Evaluate<T>(object @object)
    {
        throw new NotImplementedException();
    }
}
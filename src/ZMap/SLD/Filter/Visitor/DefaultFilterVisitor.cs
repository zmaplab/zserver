using System;
using System.Collections.Generic;
using ZMap.SLD.Expression;

namespace ZMap.SLD.Filter.Visitor;

public class DefaultFilterVisitor : IFilterVisitor, IExpressionVisitor
{
    private readonly Stack<dynamic> _stack;

    public DefaultFilterVisitor()
    {
        _stack = new Stack<dynamic>();
    }

    public object Visit(PropertyIsEqualTo filter, object extraData)
    {
        var data1 = filter.Left.Accept(this, extraData);
        var data2 = filter.Right.Accept(this, extraData);

        if (data1 == null && data2 == null)
        {
            return true;
        }

        if (data1 == null || data2 == null)
        {
            return false;
        }

        if (filter.Left is PropertyName)
        {
            var type = data1.GetType();
            var data3 = Convert.ChangeType(data2, type);
            return Equals(data1, data3);
        }
        else
        {
            var type = data2.GetType();
            var data3 = Convert.ChangeType(data1, type);
            return Equals(data2, data3);
        }
    }


    public void Push(dynamic obj)
    {
        _stack.Push(obj);
    }

    public dynamic Pop()
    {
        return _stack.Pop();
    }

    public object Visit(NilExpression expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Add expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Div expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Function expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(Literal expression, object extraData)
    {
        return expression.Evaluate(extraData);
    }

    public object Visit(Mul expression, object extraData)
    {
        throw new System.NotImplementedException();
    }

    public object Visit(PropertyName expression, object extraData)
    {
        return expression.Accept(this, extraData);
    }

    public object Visit(Sub expression, object extraData)
    {
        throw new System.NotImplementedException();
    }
}
using System.Linq;
using ZMap.SLD.Filter.Expression;
using ZMap.Style;
using ZMap.Utilities;

namespace ZMap.SLD;

public static class ParameterValueListExtensions
{
    public static ParameterValue GetOrDefault(this NamedParameter[] parameters, string name)
    {
        return parameters.FirstOrDefault(x => x.Name == name);
    }

    public static Expression<T> BuildExpression<T>(this ParameterValue parameter, IStyleVisitor visitor,
        object extraData,
        T defaultValue)
    {
        if (parameter == null)
        {
            return Expression<T>.New(defaultValue);
        }
        else
        {
            parameter.Accept(visitor, extraData);
            var value = visitor.Pop();
            return value == null ? Expression<T>.New(defaultValue) : Expression<T>.From(value, defaultValue);
        }
    }

    public static Expression<T[]> BuildArrayExpression<T>(this ParameterValue parameter, IExpressionVisitor visitor,
        object extraData,
        T[] defaultValue)
    {
        if (parameter == null)
        {
            return Expression<T[]>.New(defaultValue);
        }
        else
        {
            var text = parameter.Text?.ElementAtOrDefault(0);
            if (text == null)
            {
                foreach (var expressionType in parameter.Items)
                {
                    expressionType.Accept(visitor, extraData);
                }

                var result = visitor.Pop();
                if (result is Expression expression)
                {
                    return result as Expression<T[]> ?? Expression<T[]>.New(null, expression.Body);
                }
                else
                {
                    text = result.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Expression<T[]>.New(defaultValue);
            }
            else
            {
                var array = ConvertUtilities.ToArray<T>(text);
                return Expression<T[]>.New(array);
            }
        }
    }
}
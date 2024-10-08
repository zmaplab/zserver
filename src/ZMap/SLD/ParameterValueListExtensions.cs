namespace ZMap.SLD;

public static class ParameterValueListExtensions
{
    public static ParameterValue GetOrDefault(this NamedParameter[] parameters, string name)
    {
        return parameters.FirstOrDefault(x => x.Name == name);
    }

    public static CSharpExpressionV2<T> BuildExpressionV2<T>(this ParameterValue parameter, IStyleVisitor visitor,
        object extraData)
    {
        if (parameter == null)
        {
            return CSharpExpressionV2.Create<T>("default");
        }
        else
        {
            parameter.Accept(visitor, extraData);
            var value = visitor.Pop();
            if (value == null)
            {
                return CSharpExpressionV2.Create<T>("default");
            }

            return CSharpExpressionV2.Create<T>(value.ToString());

            // var type = typeof(T);
            // Type valueType;
            // if (type.Name == "Nullable`1" && type.Namespace == "System")
            // {
            //     valueType = type.GenericTypeArguments.ElementAt(0);
            // }
            // else
            // {
            //     valueType = type;
            // }
            //
            // return CSharpExpression<T>.From(System.Convert.ChangeType(value, valueType), defaultValue);
        }
    }

    // public static CSharpExpression<T> BuildExpression<T>(this ParameterValue parameter, IStyleVisitor visitor,
    //     object extraData,
    //     T defaultValue)
    // {
    //     if (parameter == null)
    //     {
    //         return CSharpExpression<T>.New(defaultValue);
    //     }
    //     else
    //     {
    //         parameter.Accept(visitor, extraData);
    //         var value = visitor.Pop();
    //         if (value == null)
    //         {
    //             return CSharpExpression<T>.New(defaultValue);
    //         }
    //
    //         var type = typeof(T);
    //         Type valueType;
    //         if (type.Name == "Nullable`1" && type.Namespace == "System")
    //         {
    //             valueType = type.GenericTypeArguments.ElementAt(0);
    //         }
    //         else
    //         {
    //             valueType = type;
    //         }
    //
    //         return CSharpExpression<T>.From(System.Convert.ChangeType(value, valueType), defaultValue);
    //     }
    // }

    public static CSharpExpressionV2<T> BuildArrayExpressionV2<T>(this ParameterValue parameter,
        IExpressionVisitor visitor,
        object extraData)
    {
        if (parameter == null)
        {
            return CSharpExpressionV2.Create<T>("default");
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
                if (result is CSharpExpressionV2<T>)
                {
                    // return result as CSharpExpression<T[]> ?? CSharpExpression<T[]>.New(null, expression.Expression);
                    return result;
                }

                text = result.ToString();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return CSharpExpressionV2.Create<T>("default");
            }

            var array = text.Replace(' ', ',');
            return CSharpExpressionV2.Create<T>($$"""
                                               new {{typeof(T).FullName}} { {{array}} }
                                               """);
        }
    }

    // public static CSharpExpression<T[]> BuildArrayExpression<T>(this ParameterValue parameter,
    //     IExpressionVisitor visitor,
    //     object extraData,
    //     T[] defaultValue)
    // {
    //     if (parameter == null)
    //     {
    //         return CSharpExpression<T[]>.New(defaultValue);
    //     }
    //     else
    //     {
    //         var text = parameter.Text?.ElementAtOrDefault(0);
    //         if (text == null)
    //         {
    //             foreach (var expressionType in parameter.Items)
    //             {
    //                 expressionType.Accept(visitor, extraData);
    //             }
    //
    //             var result = visitor.Pop();
    //             if (result is CSharpExpression expression)
    //             {
    //                 return result as CSharpExpression<T[]> ?? CSharpExpression<T[]>.New(null, expression.Expression);
    //             }
    //             else
    //             {
    //                 text = result.ToString();
    //             }
    //         }
    //
    //         if (string.IsNullOrWhiteSpace(text))
    //         {
    //             return CSharpExpression<T[]>.New(defaultValue);
    //         }
    //         else
    //         {
    //             var array = Convert.ToArray<T>(text);
    //             return CSharpExpression<T[]>.New(array);
    //         }
    //     }
    // }
}
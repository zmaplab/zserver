using Microsoft.Extensions.Configuration;
using ZMap.Style;

namespace ZServer.Extensions
{
    public static class ConfigurationSectionExtensions
    {
        public static Expression<T> GetExpression<T>(this IConfigurationSection section, string name)
        {
            if (name == "filter" && typeof(T) == typeof(bool?))
            {
                var expression = section.GetValue<string>(name);
                if (bool.TryParse(expression, out var result))
                {
                    return Expression<bool?>.New(result) as Expression<T>;
                }
                else
                {
                    return Expression<bool?>.New(null, expression) as Expression<T>;
                }
            }

            var expressionValue = section.GetSection($"{name}:value").Get<T>();
            var expressionBody = section.GetSection($"{name}:expression").Get<string>();
            if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
                !string.IsNullOrWhiteSpace(expressionBody))
            {
                return Expression<T>.New(default, expressionBody);
            }

            var targetSection = section.GetSection(name);
            var value = targetSection.Get<T>();
            return Expression<T>.New(value);
        }

        // public static T GetOrDefault<T>(this IConfigurationSection section, string name)
        // {
        //     var type = typeof(T);
        //     if (type.IsValueType && Nullable.GetUnderlyingType(type) != null)
        //     {
        //         type = typeof(Nullable<>).MakeGenericType(type);
        //     }
        //
        //     T value = default;
        //     var obj = section.GetSection(name).Get(type);
        //     if (obj != null)
        //     {
        //         value = (T)obj;
        //     }
        //
        //     return value;
        // }

        // public static Expression<T> GetExpression<T>(this IConfigurationSection section, string name)
        // {
        //     var expression = section.CreateExpression<T>(name);
        //     if (expression != null && !expression.IsNull())
        //     {
        //         return expression;
        //     }
        //     else
        //     {
        //         var value = section.GetValue<T>(name);
        //         return Expression<T>.New(value);
        //     }
        // }
    }
}
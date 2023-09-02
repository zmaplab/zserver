using Microsoft.Extensions.Configuration;
using ZMap.Style;

namespace ZServer.Extensions
{
    public static class ConfigurationSectionExtensions
    {
        public static CSharpExpression<T> GetExpression<T>(this IConfigurationSection section, string name)
        {
            var expressionValue = section.GetSection($"{name}:value").Get<T>();
            var expressionBody = section.GetSection($"{name}:expression").Get<string>();
            // 若表达示值不为空，或者配置的值是默认值
            if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
                !string.IsNullOrWhiteSpace(expressionBody))
            {
                return CSharpExpression<T>.New(expressionValue, expressionBody);
            }

            var targetSection = section.GetSection(name);
            var value = targetSection.Get<T>();
            return CSharpExpression<T>.New(value);
        }

        public static CSharpExpression<bool?> GetFilterExpression(this IConfigurationSection section)
        {
            var expression = section.GetValue<string>("filter");
            return bool.TryParse(expression, out var result)
                ? CSharpExpression<bool?>.New(result)
                : CSharpExpression<bool?>.New(null, string.IsNullOrEmpty(expression) ? null : expression);
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
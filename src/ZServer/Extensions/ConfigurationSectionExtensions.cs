using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ZMap.Style;

namespace ZServer.Extensions
{
    public static class ConfigurationSectionExtensions
    {
        public static Expression<T> CreateExpression<T>(this IConfigurationSection section, string name)
        {
#if DEBUG
            if (name == "dashArray")
            {
            }
#endif

            if (!section.GetSection(name).GetChildren().Any())
            {
                return default;
            }

            var value = section.GetOrDefault<T>($"{name}:value");
            var body = section.GetSection($"{name}:expression").Get<string>();
            return Expression<T>.New(value, body);
        }

        public static T GetOrDefault<T>(this IConfigurationSection section, string name)
        {
            var type = typeof(T);
            if (type.IsValueType && Nullable.GetUnderlyingType(type) != null)
            {
                type = typeof(Nullable<>).MakeGenericType(type);
            }

            T value = default;
            var obj = section.GetSection(name).Get(type);
            if (obj != null)
            {
                value = (T)obj;
            }

            return value;
        }

        public static Expression<T> Get<T>(this IConfigurationSection section, string name)
        {
            var expression = section.CreateExpression<T>(name);
            if (expression != null && !expression.IsNull())
            {
                return expression;
            }
            else
            {
                var value = section.GetOrDefault<T>(name);
                return Expression<T>.New(value);
            }
        }
    }
}
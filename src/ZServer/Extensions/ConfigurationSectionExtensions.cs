using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ZMap.Style;

namespace ZServer.Extensions;

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

    public static CSharpExpression<T> GetExpression<T>(this JToken section, string name)
    {
        var targetSection = section[name];
        switch (targetSection)
        {
            case null:
                return null;
            case JValue v:
            {
                var value = v.ToObject<T>();
                return CSharpExpression<T>.New(value);
            }
            case JArray a:
            {
                var value = a.ToObject<T>();
                return CSharpExpression<T>.New(value);
            }
        }

        var expressionValue = targetSection["value"] == null ? default : targetSection["value"].ToObject<T>();
        var expressionBody = targetSection["expression"]?.ToObject<string>();

        // 若表达示值不为空，或者配置的值是默认值
        if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
            !string.IsNullOrWhiteSpace(expressionBody))
        {
            return CSharpExpression<T>.New(expressionValue, expressionBody);
        }

        return CSharpExpression<T>.New(default);
    }

    public static CSharpExpression<bool?> GetFilterExpression(this IConfigurationSection section)
    {
        var expression = section.GetValue<string>("filter");
        return bool.TryParse(expression, out var result)
            ? CSharpExpression<bool?>.New(result)
            : CSharpExpression<bool?>.New(null, string.IsNullOrEmpty(expression) ? null : expression);
    }

    public static CSharpExpression<bool?> GetFilterExpression(this JProperty section)
    {
        var filterToken = section.Value["filter"];
        if (filterToken == null)
        {
            return CSharpExpression<bool?>.New(null);
        }

        var expression = filterToken.ToObject<string>();
        // // C# Script
        // if (expression.StartsWith("{{"))
        // {
        //     
        // }
        // else
        // {
        //     
        // }
        return bool.TryParse(expression, out var result)
            ? CSharpExpression<bool?>.New(result)
            : CSharpExpression<bool?>.New(null, string.IsNullOrEmpty(expression) ? null : expression);
    }

    public static CSharpExpression<bool?> GetFilterExpression(this JObject section)
    {
        var filterToken = section["filter"];
        if (filterToken == null)
        {
            return CSharpExpression<bool?>.New(null);
        }

        var expression = filterToken.ToObject<string>();
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
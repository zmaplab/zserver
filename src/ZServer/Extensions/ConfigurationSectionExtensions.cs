using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ZMap.Style;

namespace ZServer.Extensions;

public static class ConfigurationSectionExtensions
{
    // public static CSharpExpression<T> GetExpression<T>(this IConfigurationSection section, string name)
    // {
    //     var expressionValue = section.GetSection($"{name}:value").Get<T>();
    //     var expressionBody = section.GetSection($"{name}:expression").Get<string>();
    //     // 若表达示值不为空，或者配置的值是默认值
    //     if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
    //         !string.IsNullOrWhiteSpace(expressionBody))
    //     {
    //         return CSharpExpression<T>.New(expressionValue, expressionBody);
    //     }
    //
    //     var targetSection = section.GetSection(name);
    //     var value = targetSection.Get<T>();
    //     return CSharpExpression<T>.New(value);
    // }

    // public static CSharpExpression<T> GetExpression<T>(this JToken section, string name)
    // {
    //     var targetSection = section[name];
    //     switch (targetSection)
    //     {
    //         case null:
    //             return CSharpExpression<T>.New(default);
    //         case JValue v:
    //         {
    //             var value = v.ToObject<T>();
    //             return CSharpExpression<T>.New(value);
    //         }
    //         case JArray a:
    //         {
    //             var value = a.ToObject<T>();
    //             return CSharpExpression<T>.New(value);
    //         }
    //     }
    //
    //     var expressionValue = targetSection["value"] == null ? default : targetSection["value"].ToObject<T>();
    //     var expressionBody = targetSection["expression"]?.ToObject<string>();
    //
    //     // 若表达示值不为空，或者配置的值是默认值
    //     if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
    //         !string.IsNullOrWhiteSpace(expressionBody))
    //     {
    //         return CSharpExpression<T>.New(expressionValue, expressionBody);
    //     }
    //
    //     return CSharpExpression<T>.New(default);
    // }

    public static CSharpExpressionV2<T> GetExpressionV2<T>(this JToken section, string name)
    {
        var targetSection = section[name];
        return targetSection switch
        {
            null => CSharpExpressionV2.Create<T>("{{ default }}"),
            JValue v => CSharpExpressionV2.Create<T>(v.ToObject<string>() ?? "{{ default }}"),
            JArray array => CreateArrayExpression<T>(array),
            _ => throw new ArgumentException("Invalid expression value")
        };

        // var expressionValue = targetSection["value"] == null ? default : targetSection["value"].ToObject<T>();
        // var expressionBody = targetSection["expression"]?.ToObject<string>();
        //
        // // 若表达示值不为空，或者配置的值是默认值
        // if ((expressionValue != null && !expressionValue.Equals(default(T))) ||
        //     !string.IsNullOrWhiteSpace(expressionBody))
        // {
        //     return CSharpExpression<T>.New(expressionValue, expressionBody);
        // }
        //
        // return CSharpExpression<T>.New(default);
    }

    private static CSharpExpressionV2<T> CreateArrayExpression<T>(JArray array)
    {
        var type = typeof(T);
        if (type.IsArray)
        {
            if (type.FullName == "System.String[]")
            {
                return CSharpExpressionV2.Create<T>($$"""
                                                      new {{type.FullName}} { {{string.Join(',', array.Select(x => $"\"{x}\""))}} }
                                                      """);
            }

            return CSharpExpressionV2.Create<T>($$"""
                                                   new {{type.FullName}} { {{string.Join(',', array)}} }
                                                  """);
        }

        if (type.IsGenericType)
        {
            if (type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0] == typeof(string))
            {
                return CSharpExpressionV2.Create<T>(
                    $$"""
                        new System.Collections.Generic.List<System.String> { {{string.Join(',', array.Select(x => $"\"{x}\""))}} }
                      """);
            }
        }

        throw new ArgumentException($"不支持的数据类型 {type.FullName}");
    }

    // public static CSharpExpression<bool?> GetFilterExpression(this IConfigurationSection section)
    // {
    //     var expression = section.GetValue<string>("filter");
    //     return bool.TryParse(expression, out var result)
    //         ? CSharpExpression<bool?>.New(result)
    //         : CSharpExpression<bool?>.New(null, string.IsNullOrEmpty(expression) ? null : expression);
    // }

    // public static CSharpExpression<bool?> GetFilterExpression(this JProperty section)
    // {
    //     var filterToken = section.Value["filter"];
    //     if (filterToken == null)
    //     {
    //         return CSharpExpression<bool?>.New(null);
    //     }
    //
    //     var expression = filterToken.ToObject<string>();
    //     // // C# Script
    //     // if (expression.StartsWith("{{"))
    //     // {
    //     //     
    //     // }
    //     // else
    //     // {
    //     //     
    //     // }
    //     return bool.TryParse(expression, out var result)
    //         ? CSharpExpression<bool?>.New(result)
    //         : CSharpExpression<bool?>.New(null, string.IsNullOrEmpty(expression) ? null : expression);
    // }

    public static CSharpExpressionV2<bool?> GetFilterExpressionV2(this JObject section)
    {
        var filterToken = section["filter"] as JValue;
        if (filterToken == null || filterToken.Value == null)
        {
            // 未配置过滤器则默认为 true
            return CSharpExpressionV2.Create<bool?>("true");
        }

        if (filterToken.Value is bool value)
        {
            return CSharpExpressionV2.Create<bool?>(value ? "true" : "false");
        }

        var expression = filterToken.Value.ToString();
        return CSharpExpressionV2.Create<bool?>(expression);
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
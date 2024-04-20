// using Newtonsoft.Json;
using ZMap.Infrastructure;

namespace ZMap.Style;

/// <summary>
/// 1.若有 value，没有表达式，则使用 value
/// 2.若有 value，有表达式，表达式的值不为空，则使用表达式的值
/// 3.若无 value，没有表达式，则使用表达式的值
/// 4.若无 value，有表达式，则使用表达式的值
/// </summary>
/// <typeparam name="TV"></typeparam>
public class CSharpExpression<TV> : CSharpExpression
{
    /// <summary>
    /// 
    /// </summary>
    // [JsonProperty]
    public TV Value { get; internal set; }

    public static CSharpExpression<TV> New(TV v, string body = null)
    {
        return new CSharpExpression<TV>
        {
            Value = v,
            Expression = body
        };
    }

    public static CSharpExpression<TV> From(TV value, TV defaultValue)
    {
        if (value == null)
        {
            return New(defaultValue);
        }
        else
        {
            var v = ConvertUtilities.ToObject<TV>(value);
            return New(Equals(v, default(TV)) ? defaultValue : v);
        }
    }

    public CSharpExpression<TV> Clone()
    {
        return New(Value, Expression);
    }

    public void Invoke(Feature feature, TV defaultValue = default)
    {
        if (Value != null)
        {
            if (!string.IsNullOrWhiteSpace(Expression))
            {
                // 只要有表达式，则以表达式优先，不然何必去配置表达式呢
                // 即使表达式计算的值为空，也应该认为是用户需要的结果
                var func = CSharpDynamicCompiler.GetOrCreateFunc(Expression);
                if (func == null)
                {
                    return;
                }

                var output = func.Invoke(feature);
                if (output != null)
                {
                    Value = output;
                }
            }
        }
        else
        {
            // 说明未直接配值，在表达式对象中也没有配置值
            if (string.IsNullOrWhiteSpace(Expression))
            {
                Value = defaultValue;
            }
            else
            {
                var func = CSharpDynamicCompiler.GetOrCreateFunc(Expression);
                if (func != null)
                {
                    var output = func.Invoke(feature);
                    Value = output != null ? (TV)output : defaultValue;
                }
                else
                {
                    Value = defaultValue;
                }
            }
        }
    }
}

public class CSharpExpression
{
    /// <summary>
    /// 
    /// </summary>
    // [JsonProperty]
    public string Expression { get; internal set; }

    public static CSharpExpression New(string body = null)
    {
        return new CSharpExpression
        {
            Expression = body
        };
    }
}
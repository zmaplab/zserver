using System;
using ZMap.Infrastructure;

namespace ZMap.Style
{
    /// <summary>
    /// 1.若有 value，没有表达式，则使用 value
    /// 2.若有 value，有表达式，表达式的值不为空，则使用表达式的值
    /// 3.若无 value，没有表达式，则使用表达式的值
    /// 4.若无 value，有表达式，则使用表达式的值
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    public class CSharpExpression<TV> : CSharpExpression
    {
        public TV Value { get; internal set; }

        public static CSharpExpression<TV> New(TV v, string body = null)
        {
            return new CSharpExpression<TV>
            {
                Value = v,
                Body = body
            };
        }

        public static CSharpExpression<TV> From(dynamic value, TV defaultValue)
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

        public void Invoke(Feature feature, TV defaultValue = default)
        {
            if (Value != null)
            {
                if (string.IsNullOrWhiteSpace(Body))
                {
                }
                else
                {
                    var func = DynamicCompilationUtilities.GetOrCreateFunc(Body);
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
                if (string.IsNullOrWhiteSpace(Body))
                {
                    Value =  defaultValue;
                }
                else
                {
                    var func = DynamicCompilationUtilities.GetOrCreateFunc(Body);
                    if (func != null)
                    {
                        var output = func.Invoke(feature);
                        Value = output != null ? (TV)output : defaultValue;
                    }
                    else
                    {
                        Value =  defaultValue;
                    }
                }
            }
        }
    }

    public class CSharpExpression
    {
        public string Body { get; protected set; }

        public static CSharpExpression New(string body = null)
        {
            return new CSharpExpression
            {
                Body = body
            };
        }
    }
}
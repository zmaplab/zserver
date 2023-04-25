using ZMap.Infrastructure;

namespace ZMap.Style
{
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
            if (string.IsNullOrWhiteSpace(Body))
            {
                return;
            }

            if (Value != null && !Value.Equals(default))
            {
                return;
            }

            var func = DynamicCompilationUtilities.GetOrCreateFunc(Body);
            if (func == null)
            {
                Value = defaultValue;
            }
            else
            {
                var result = func.Invoke(feature);
                Value = result is TV tv ? tv : defaultValue;
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
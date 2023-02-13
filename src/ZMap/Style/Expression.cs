using ZMap.Utilities;

namespace ZMap.Style
{
    public class Expression<TV> : Expression
    {
        public TV Value { get; internal set; }

        public static Expression<TV> New(TV v, string body = null)
        {
            return new Expression<TV>
            {
                Value = v,
                Body = body
            };
        }

        public static Expression<TV> From(dynamic value, TV defaultValue)
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

            var func = DynamicCompilationUtilities.GetFunc(Body);
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

    public class Expression
    {
        public string Body { get; protected set; }

        public static Expression New(string body = null)
        {
            return new Expression
            {
                Body = body
            };
        }
    }
}
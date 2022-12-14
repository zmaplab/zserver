using ZMap.Utilities;

namespace ZMap.Style
{
    public class Expression<TV>
    {
        public TV Value { get; internal set; }

        public string Body { get; private set; }

        public static Expression<TV> New(TV v, string body = null)
        {
            return new Expression<TV>
            {
                Value = v,
                Body = body
            };
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
}
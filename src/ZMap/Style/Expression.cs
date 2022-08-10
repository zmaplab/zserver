namespace ZMap.Style
{
    public class Expression<TV>
    {
        public TV Value { get; set; }

        public string Body { get; set; }

        public static Expression<TV> New(TV v, string body = null)
        {
            return new Expression<TV>
            {
                Value = v,
                Body = body
            };
        }

        public bool IsNull()
        {
            return Value == null && string.IsNullOrWhiteSpace(Body);
        }
    }
}
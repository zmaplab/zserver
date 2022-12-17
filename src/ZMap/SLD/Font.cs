using System.Collections.Generic;
using ZMap.Style;

namespace ZMap.SLD
{
    public class Font : ParameterCollection
    {
        private static readonly Dictionary<string, dynamic> Defaults = new();

        static Font()
        {
            Defaults.Add("font-family", null);
            Defaults.Add("font-size", 12);
            Defaults.Add("font-weight", 0);
            Defaults.Add("font-style", null);
        }

        /// <summary>
        /// 字体宽
        /// </summary>
        public ParameterValue Weight => GetParameterValue<string>("font-weight");

        /// <summary>
        /// 字体大小
        /// </summary>
        public ParameterValue Size => GetParameterValue<int>("font-size");

        /// <summary>
        /// 字体
        /// </summary>
        public ParameterValue Family => GetParameterValue<string>("font-family");

        /// <summary>
        /// 斜体等
        /// </summary>
        public ParameterValue Style => GetParameterValue<string>("font-style");

        protected override T GetDefault<T>(string name)
        {
            return Defaults.ContainsKey(name) ? Defaults[name] : default;
        }

        public void Accept(IStyleVisitor visitor, object extraData)
        {
            if (visitor.Pop() is not TextStyle parent)
            {
                return;
            }

            parent.Size = Accept<int>(Size, visitor, extraData);
            var expression = Accept<string>(Family, visitor, extraData);

            parent.Font = !string.IsNullOrWhiteSpace(expression.Value)
                ? Expression<string[]>.New(new string[] { expression.Value })
                : Expression<string[]>.New(null, $"new string[] {{ {expression.Body} }}");

            visitor.Push(parent);
        }
    }
}
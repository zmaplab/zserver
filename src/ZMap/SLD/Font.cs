using System.Collections.Generic;

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
        public string Weight => Get<string>("font-weight");

        /// <summary>
        /// 字体大小
        /// </summary>
        public int Size => Get<int>("font-size");

        /// <summary>
        /// 字体
        /// </summary>
        public string Family => Get<string>("font-family");

        /// <summary>
        /// 斜体等
        /// </summary>
        public string Style => Get<string>("font-style");

        protected override T GetDefault<T>(string name)
        {
            return Defaults.ContainsKey(name) ? Defaults[name] : default;
        }
    }
}
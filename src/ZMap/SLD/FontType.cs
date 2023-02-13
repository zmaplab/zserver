using System.Collections.Generic;

namespace ZMap.SLD
{
    public class FontType
    {
        internal static readonly Dictionary<string, dynamic> Defaults = new();

        static FontType()
        {
            Defaults.Add("font-family", null);
            Defaults.Add("font-size", 12);
            Defaults.Add("font-weight", 0);
            Defaults.Add("font-style", null);
        }
    }
}
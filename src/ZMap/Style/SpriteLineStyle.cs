using System;

namespace ZMap.Style
{
    public class SpriteLineStyle : LineStyle
    {
        public Expression<Uri> Resource { get; set; }
        public Expression<string> Pattern { get; set; }
    }
}
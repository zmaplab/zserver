using System;

namespace ZMap.Style
{
    public class SpriteLineStyle : LineStyle
    {
        public Expression<Uri> Uri { get; set; }
        public Expression<string> Pattern { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Pattern?.Invoke(feature);
            Uri?.Invoke(feature);
        }
    }
}
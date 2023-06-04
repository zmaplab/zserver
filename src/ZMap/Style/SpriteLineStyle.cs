using System;

namespace ZMap.Style
{
    public class SpriteLineStyle : LineStyle
    {
        public CSharpExpression<Uri> Uri { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Uri?.Invoke(feature);
        }
        
        public override Style Clone()
        {
            return new SpriteLineStyle
            {
                MaxZoom = MaxZoom,
                MinZoom = MinZoom,
                ZoomUnit = ZoomUnit,
                Opacity = Opacity?.Clone(),
                Pattern = Pattern?.Clone(),
                Color = Color?.Clone(),
                Translate = Translate?.Clone(),
                TranslateAnchor = TranslateAnchor?.Clone(),
                Width = Width?.Clone(),
                DashArray = DashArray?.Clone(),
                DashOffset = DashOffset?.Clone(),
                LineJoin = LineJoin?.Clone(),
                LineCap = LineCap?.Clone(),
                GapWidth = GapWidth?.Clone(),
                Offset = Offset?.Clone(),
                Blur = Blur?.Clone(),
                Gradient = Gradient?.Clone(),
                Uri = Uri?.Clone()
            };
        }
    }
}
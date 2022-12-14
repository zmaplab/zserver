using System;
using System.Diagnostics.CodeAnalysis;

namespace ZMap.Style
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SymbolStyle : VectorStyle
    {
        public Expression<int> Size { get; set; }
        public Expression<Uri> Uri { get; set; }
        public Expression<float> Opacity { get; set; }
        public Expression<float> Rotation { get; set; }

        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Size?.Invoke(feature, 24);
            Uri?.Invoke(feature);
            Opacity?.Invoke(feature, 1);
            Rotation?.Invoke(feature);
        }
    }
}
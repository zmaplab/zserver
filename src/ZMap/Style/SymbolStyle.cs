using System;
using System.Diagnostics.CodeAnalysis;

namespace ZMap.Style
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SymbolStyle : VectorStyle
    {
        public Expression<int> Size { get; set; }
        public  Expression<Uri> Uri { get; set; }
    }
}
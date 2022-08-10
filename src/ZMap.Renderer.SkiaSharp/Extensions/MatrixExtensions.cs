using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.Extensions
{
    public static class MatrixExtensions
    {
        public static SKMatrix Concat(this SKMatrix first, SKMatrix second)
        {
            var target = SKMatrix.CreateIdentity();
            SKMatrix.Concat(ref target, first, second);
            return target;
        }
    }
}
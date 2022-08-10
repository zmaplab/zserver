// using SkiaSharp;
//
// namespace ZMap.Renderer.SkiaSharp.Extensions
// {
//     public class SKPointExtensions
//     {
//         public static void Adject(this SKPoint centroidPoint, int width, int height)
//         {
//             var left = centroidPoint.X - half;
//             var top = centroidPoint.Y - half;
//             var right = centroidPoint.X + half;
//             var bottom = centroidPoint.Y + half;
//             if (left < 0)
//             {
//                 right += Math.Abs(left);
//                 left = 0;
//             }
//
//             if (top < 0)
//             {
//                 bottom += Math.Abs(top);
//                 top = 0;
//             }
//
//             if (right > width)
//             {
//                 left -= right - width;
//                 right = width;
//             }
//
//             if (bottom > height)
//             {
//                 top -= bottom - height;
//                 bottom = height;
//             }
//
//             var rect = new SKRect(left, top, right, bottom);
//         }
//     }
// }
// using System;
//
// namespace ZServer.Interfaces
// {
//     public record XYZ
//     {
//         public int X;
//         public int Y;
//         public int Z;
//
//         public static XYZ Caculate(double originX, double originY, int zoom, double x, double y, int tileSize = 256)
//         {
//             var res = (originY - originX) / tileSize / Math.Pow(2, zoom);
//             var size = res * tileSize; // 4891.9698095703125
//             var x2 = (int) Math.Floor((x - originX) / size);
//             var y2 = (int) Math.Floor((originY - y) / size);
//             return new XYZ
//             {
//                 Z = zoom,
//                 X = x2,
//                 Y = y2
//             };
//         }
//
//         public override string ToString()
//         {
//             return $"{Z}/{X}/{Y}";
//         }
//     }
// }
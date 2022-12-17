// using System;
//
// namespace ZMap.Extensions;
//
// public static class ArrayExtensions
// {
//     public static int FindIndex<T>(this T[] array, Func<T, bool> match)
//     {
//         for (var i = 0; i < array.Length; ++i)
//         {
//             var item = array[i];
//             if (match(item))
//             {
//                 return i;
//             }
//         }
//
//         return -1;
//     }
// }
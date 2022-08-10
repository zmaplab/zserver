// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using Natasha.CSharp;
// using ZMap.Source;
//
// namespace ZServer.Utilities
// {
//     public static class DynamicCompilationUtilities
//     {
//         private static readonly ConcurrentDictionary<string, Func<Feature, bool>>
//             AvailableCache;
//
//         static DynamicCompilationUtilities()
//         {
//             AvailableCache = new ConcurrentDictionary<string, Func<Feature, bool>>();
//         }
//
//         public static Func<Feature, bool> GetAvailableFunc(string expression)
//         {
//             if (string.IsNullOrWhiteSpace(expression))
//             {
//                 return null;
//             }
//
//             return AvailableCache.GetOrAdd(expression, x =>
//             {
//                 var body = $"return {x};";
//                 var f = FastMethodOperator.DefaultDomain()
//                     .Param(typeof(Feature), "feature")
//                     .Body(body)
//                     .Compile<Func<Feature, bool>>();
//                 return f;
//             });
//         }
//
//         // ReSharper disable once InconsistentNaming
//         public static Func<Feature, bool> GetCQLFunc(string expression)
//         {
//             if (string.IsNullOrWhiteSpace(expression))
//             {
//                 return null;
//             }
//
//             return AvailableCache.GetOrAdd(expression, e1 =>
//             {
//                 var body = $"return {e1};";
//
//                 // xzq_dm >= 100 AND xzq_dm < 1000
//                 var pieces = e1.Split(new[] { ">=", ">", "<=", "<", "AND", "OR", "=" },
//                     StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
//
//                 var duplicate = new HashSet<string>();
//                 for (var i = 0; i < pieces.Count; ++i)
//                 {
//                     if (i % 2 == 0)
//                     {
//                         var piece = pieces[i];
//                         if (duplicate.Contains(piece))
//                         {
//                             continue;
//                         }
//
//                         body = body.Replace(piece, $" feature[\"{piece}\"] ");
//                         duplicate.Add(piece);
//                     }
//                 }
//
//                 body = body.Replace("AND", " && ").Replace("OR", " || ");
//                 var func = FastMethodOperator.DefaultDomain().Param(typeof(Feature), "feature")
//                     .Body(body)
//                     .Compile<Func<Feature, bool>>();
//                 return func;
//             });
//         }
//     }
// }
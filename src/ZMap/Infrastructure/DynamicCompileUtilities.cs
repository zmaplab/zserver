using System;
using System.Collections.Concurrent;

namespace ZMap.Infrastructure
{
    public static class DynamicCompilationUtilities
    {
        public static ICSharpDynamicCompiler Compiler;

        private static readonly ConcurrentDictionary<string, dynamic>
            FuncCache;

        static DynamicCompilationUtilities()
        {
            FuncCache = new ConcurrentDictionary<string, dynamic>();
        }

        public static void Load(ICSharpDynamicCompiler compiler)
        {
            Compiler = compiler;
        }

        public static Func<Feature, dynamic> GetOrCreateFunc(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || Compiler == null)
            {
                return null;
            }

            return FuncCache.GetOrAdd(expression, _ => Compiler.BuildFunc(expression));
        }

        public static Func<Feature, T> GetOrCreateFunc<T>(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || Compiler == null)
            {
                return null;
            }

            return FuncCache.GetOrAdd(expression, _ => Compiler.BuildFunc<T>(expression));
        }
    }
}
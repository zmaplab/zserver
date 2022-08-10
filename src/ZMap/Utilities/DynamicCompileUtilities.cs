using System;
using System.Collections.Concurrent;
using ZMap.Source;

namespace ZMap.Utilities
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

        public static Func<Feature, bool> GetAvailableFunc(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || Compiler == null)
            {
                return null;
            }

            return FuncCache.GetOrAdd(expression, _ => GetFunc<bool>(expression));
        }

        public static Func<Feature, T> GetFunc<T>(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || Compiler == null)
            {
                return null;
            }

            return FuncCache.GetOrAdd(expression, _ => Compiler.Build<T>(expression));
        }
    }
}
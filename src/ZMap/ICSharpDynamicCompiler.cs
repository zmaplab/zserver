using System;
using ZMap.Source;

namespace ZMap;

public interface ICSharpDynamicCompiler
{
    Func<Feature, T> Build<T>(string script);
}
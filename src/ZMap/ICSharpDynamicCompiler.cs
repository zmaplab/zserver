using System;

namespace ZMap;

public interface ICSharpDynamicCompiler
{
    Func<Feature, dynamic> Build(string script);
    Func<Feature, T> Build<T>(string script);
}
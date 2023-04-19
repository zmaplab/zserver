using System;

namespace ZMap.Infrastructure;

public interface ICSharpDynamicCompiler
{
    Func<Feature, dynamic> BuildFunc(string script);
    Func<Feature, T> BuildFunc<T>(string script);
    Type BuildType(string script);
}
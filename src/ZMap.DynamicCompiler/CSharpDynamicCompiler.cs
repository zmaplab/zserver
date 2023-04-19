using Natasha.CSharp;
using ZMap.Infrastructure;

namespace ZMap.DynamicCompiler;

public class CSharpDynamicCompiler : ICSharpDynamicCompiler
{
    public Func<Feature, dynamic> BuildFunc(string script)
    {
        var body = script.EndsWith(";")
            ? script
            : $"""
return {script};
""";
        var f = FastMethodOperator.DefaultDomain()
            .Param(typeof(Feature), "feature")
            .Body(body)
            .Compile<Func<Feature, dynamic>>();

        return f;
    }

    public Type BuildType(string script)
    {
        var nClass = NClass.DefaultDomain();

        nClass.Body(script);
        return nClass.GetType();
    }

    public Func<Feature, T> BuildFunc<T>(string script)
    {
        var body = $"""
return {script};
""";
        var f = FastMethodOperator.DefaultDomain()
            .Param(typeof(Feature), "feature")
            .Body(body)
            .Compile<Func<Feature, T>>();

        return f;
    }

    public static void Initialize()
    {
        if (DynamicCompilationUtilities.Compiler != null)
        {
            return;
        }

        lock (typeof(DynamicCompilationUtilities))
        {
            if (DynamicCompilationUtilities.Compiler != null)
            {
                return;
            }

            NatashaManagement.Preheating();
            DynamicCompilationUtilities.Load(new CSharpDynamicCompiler());
        }
    }
}
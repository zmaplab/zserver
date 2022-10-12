using Natasha.CSharp;
using ZMap.Source;
using ZMap.Utilities;

namespace ZMap.DynamicCompiler;

public class CSharpDynamicCompiler : ICSharpDynamicCompiler
{
    public Func<Feature, T> Build<T>(string script)
    {
        var body = $"return {script};";
        var f = FastMethodOperator.DefaultDomain()
            .Param(typeof(Feature), "feature")
            .Body(body)
            .Compile<Func<Feature, T>>();

        return f;
    }

    public static void Load()
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
            // NatashaInitializer.Preheating();
            // NatashaManagement.AddGlobalReference()
            DynamicCompilationUtilities.Load(new CSharpDynamicCompiler());
        }
    }
}
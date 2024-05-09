using Natasha.CSharp.Template;
using ZMap.Infrastructure;

namespace ZMap.DynamicCompiler;

public class NatashaDynamicCompiler : CSharpDynamicCompiler
{
    protected override Func<Feature, dynamic> BuildFunc(string script)
    {
        var body = script.EndsWith(";")
            ? script
            : $"""
               return {script};
               """;

        var f = FastMethodOperator.DefaultDomain()
            .Param(typeof(Feature), "feature")
            .Using("System")
            .Using("System.Collections")
            .Using("System.Collections.Generic")
            .Using("System.Linq")
            .Using("System.Text.Json")
            .Using("NetTopologySuite.Features")
            .Using("Newtonsoft.Json.Linq")
            .Body(body)
            .Compile<Func<Feature, dynamic>>();
        return f;
    }

    // public override Type BuildType(string script)
    // {
    //     var nClass = NClass.DefaultDomain();
    //     nClass.Body(script);
    //     return nClass.GetType();
    // }

    protected override Func<Feature, T> BuildFunc<T>(string script)
    {
        var type = typeof(T).GetDevelopName();
        var body = $"""
                    return ({type})({script});
                    """;

        var f = FastMethodOperator.DefaultDomain()
            .Param(typeof(Feature), "feature")
            .Using("System")
            .Using("System.Collections")
            .Using("System.Collections.Generic")
            .Using("System.Linq")
            .Using("System.Text.Json")
            .Using("NetTopologySuite.Features")
            .Using("Newtonsoft.Json.Linq")
            .Body(body)
            .Compile<Func<Feature, T>>();
        return f;
    }

    protected override void Initialize()
    {
        NatashaManagement.RegistDomainCreator<NatashaDomainCreator>();
        NatashaManagement.Preheating(false, false);
    }
}
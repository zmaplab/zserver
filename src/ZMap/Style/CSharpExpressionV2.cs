namespace ZMap.Style;

public partial class CSharpExpressionV2
{
    private static readonly ConcurrentDictionary<string, dynamic> FuncCache = new();
    private const string CodeTemplate = @"^\s*\{\{|\}\}\s*$";

// #if DEBUG
//     public string Script { get; protected set; }
// #endif

    public static CSharpExpressionV2<T> Create<T>(string script)
    {
        if (string.IsNullOrEmpty(script))
        {
            return new CSharpExpressionV2<T>(null);
        }

        var type = typeof(T);
        var typeName = type.FullName;
        if (typeName == null)
        {
            return new CSharpExpressionV2<T>(null);
        }

        var func = FuncCache.GetOrAdd($"{script.GetHashCode()}:{typeName.GetHashCode()}",
            _ =>
            {
                var regex = CodeTemplateRegex();
                // C# 表达式
                if (regex.IsMatch(script))
                {
                    var body = regex.Replace(script, "");
                    return CSharpDynamicCompiler.GetOrCreateFuncWithoutCache<T>(body);
                }
                else
                {
                    string body;
                    if (type == typeof(string))
                    {
                        body = $$"""
                                 "{{script}}"
                                 """;
                    }
                    else if (type.IsEnum)
                    {
                        body = $$"""
                                 ({{type.FullName}})(Enum.TryParse(typeof({{type.FullName}}), "{{script}}", out var v) ? v : default)
                                 """;
                    }
                    else
                    {
                        body = script;
                    }

                    return CSharpDynamicCompiler.GetOrCreateFuncWithoutCache<T>(body);
                }
            });
        return new CSharpExpressionV2<T>(func)
        {
// #if DEBUG
//             Script = script
// #endif
        };
    }

    [GeneratedRegex(CodeTemplate)]
    private static partial Regex CodeTemplateRegex();

    public string FuncName { get; protected init; }

    public virtual CSharpExpressionV2 Clone()
    {
        throw new NotImplementedException();
    }
}

public class CSharpExpressionV2<T> : CSharpExpressionV2
{
    private readonly Func<Feature, T> _func;

    public CSharpExpressionV2(Func<Feature, T> func)
    {
        _func = func;
        if (_func != null)
        {
            FuncName = $"{_func.Method.ReflectedType?.FullName}.{_func.Method.Name}";
        }
    }

    public T Value { get; private set; }

    // label: 1
    // label: "1"
    // label: "return feature[\"name\"] as string;"

    public void Accept(Feature feature, T defaultValue = default)
    {
        if (_func == null)
        {
            Value = defaultValue;
            return;
        }

        var value = _func.Invoke(feature);
        Value = value == null ? defaultValue : value;
    }

    public override CSharpExpressionV2<T> Clone()
    {
        return new CSharpExpressionV2<T>(_func)
        {
// #if DEBUG
//             Script = Script
//
// #endif
        };
    }
}
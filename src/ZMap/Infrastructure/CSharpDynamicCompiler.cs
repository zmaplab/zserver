namespace ZMap.Infrastructure;

public abstract class CSharpDynamicCompiler
{
    private static CSharpDynamicCompiler _instance;

    // protected abstract Func<Feature, dynamic> BuildFunc(string script);
    protected abstract Func<Feature, T> BuildFunc<T>(string script);

    protected abstract void Initialize();

    // private static readonly ConcurrentDictionary<string, dynamic>
    //     FuncCache;

    // static CSharpDynamicCompiler()
    // {
        // FuncCache = new ConcurrentDictionary<string, dynamic>();
    // }

    public static void Load<T>() where T : CSharpDynamicCompiler
    {
        if (_instance != null)
        {
            return;
        }

        lock (typeof(CSharpDynamicCompiler))
        {
            if (_instance != null)
            {
                return;
            }

            _instance = Activator.CreateInstance<T>();
            _instance.Initialize();
        }
    }

    // public static Func<Feature, dynamic> GetOrCreateFunc(string expression)
    // {
    //     if (string.IsNullOrWhiteSpace(expression) || _instance == null)
    //     {
    //         return null;
    //     }
    //
    //     return FuncCache.GetOrAdd(expression.GetHashCode().ToString(), _ => _instance.BuildFunc(expression));
    // }

    // public static Func<Feature, T> GetOrCreateFunc<T>(string expression)
    // {
    //     if (string.IsNullOrWhiteSpace(expression) || _instance == null)
    //     {
    //         return null;
    //     }
    //
    //     return FuncCache.GetOrAdd($"{expression.GetHashCode()}:{typeof(T).FullName}",
    //         _ => _instance.BuildFunc<T>(expression));
    // }

    public static Func<Feature, T> GetOrCreateFuncWithoutCache<T>(string expression)
    {
        return _instance?.BuildFunc<T>(expression);
    }
}
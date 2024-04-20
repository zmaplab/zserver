

// ReSharper disable InconsistentNaming

namespace ZMap.Source;

public record FreeSqlFilter(string Script) : IFeatureFilter
{
    // private static readonly ConcurrentDictionary<string, Func<Feature, bool>> Cache = new();


    // public string ToQuerySql()
    // {
    //     return Script;
    // }

    // public Func<Feature, bool> ToPredicate()
    // {
    //     if (string.IsNullOrWhiteSpace(Script))
    //     {
    //         return null;
    //     }
    //
    //     return Cache.GetOrAdd(Script, expression =>
    //     {
    //         var body = $"return {expression};";
    //
    //         // xzq_dm >= 100 AND xzq_dm < 1000
    //         var pieces = expression.Split(new[] { ">=", ">", "<=", "<", "AND", "and", "or", "OR", "=" },
    //             StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
    //
    //         var duplicate = new HashSet<string>();
    //         for (var i = 0; i < pieces.Count; ++i)
    //         {
    //             if (i % 2 != 0)
    //             {
    //                 continue;
    //             }
    //
    //             var piece = pieces[i];
    //             if (duplicate.Contains(piece))
    //             {
    //                 continue;
    //             }
    //
    //             body = body.Replace(piece, $" feature[\"{piece}\"] ");
    //             duplicate.Add(piece);
    //         }
    //
    //         body = body.Replace("and", " && ").Replace("AND", " && ")
    //             .Replace("or", " || ").Replace("OR", " || ");
    //         var func = CSharpDynamicCompiler.GetOrCreateFunc<bool>(body);
    //         return func;
    //     });
    // }

    public override string ToString()
    {
        return Script;
    }
}
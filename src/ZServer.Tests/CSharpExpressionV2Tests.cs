using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using NetTopologySuite.Geometries;
using Xunit;
using ZMap;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
using ZMap.Style;

namespace ZServer.Tests;

public class CSharpExpressionV2Tests
{
    public CSharpExpressionV2Tests()
    {
        CSharpDynamicCompiler.Load<NatashaDynamicCompiler>();
    }

    private static readonly Geometry Geom = new Point(1, 1);

    private CSharpExpressionV2<T> Test<T>(string script, T expected, (string Key, dynamic Value) kv = default)
    {
        var exp = CSharpExpressionV2.Create<T>(script);
        exp.Accept(new Feature(Geom,
            kv == default ? null : new Dictionary<string, dynamic> { { kv.Key, kv.Value } }));
        Assert.Equal(expected, exp.Value);
        return exp;
    }

    private void Test<T>(string script, T expected, (string Key, dynamic Value) kv, T defaultValue)
    {
        var exp = CSharpExpressionV2.Create<T>(script);
        exp.Accept(new Feature(Geom,
            kv == default ? null : new Dictionary<string, dynamic> { { kv.Key, kv.Value } }), defaultValue);
        Assert.Equal(expected, exp.Value);
    }

    [Fact]
    public void BuildInBuild()
    {
        var exp = Test("11", 11);
        var script = $"{exp.FuncName}(feature)";
        var func = CSharpDynamicCompiler.GetOrCreateFuncWithoutCache<int>(script);

        var actual = func.Invoke(new Feature(Geom,
            new Dictionary<string, dynamic> { { "v", 20 } }));
        Assert.Equal(11, actual);
    }

    [Fact]
    public void Int()
    {
        Test("1", 1);
        Test("2", 2);
        Test("default", 0);
        Test("{{ default }}", 0);
        Test("", 23, default, 23);
        Test(null, 23, default, 23);
        Test("{{ feature[\"v\"] }}", 20, ("v", 20));
        Test("{{ feature[\"v\"] + 21 }}", 41, ("v", 20));

        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1); });
        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1, ("22", 22)); });
    }

    [Fact]
    public void IntNullable()
    {
        Test<int?>("1", 1);
        Test<int?>("2", 2);
        Test<int?>("default", null);
        Test<int?>("{{ default }}", null);
        Test<int?>("null", null);
        Test<int?>("{{ null }}", null);
        Test<int?>("", 23, default, 23);
        Test<int?>(null, 23, default, 23);
        Test<int?>("{{ feature[\"v\"] }}", 20, ("v", 20));
        Test<int?>("{{ feature[\"v\"] }}", null, ("22", 22));
        Test<int?>("{{ feature[\"v\"] + 21 }}", 41, ("v", 20));
        Test<int?>("{{ feature[\"v\"] }}", null, ("22", 22));
        Test<int?>("{{ feature[\"v\"] + 21 }}", null, ("22", 22));
    }

    [Fact]
    public void Float()
    {
        Test("1", 1f);
        Test("2", 2f);
        Test("default", 0f);
        Test("{{ default }}", 0f);
        Test("", 23f, default, 23f);
        Test(null, 23f, default, 23f);
        Test("{{ feature[\"v\"] }}", 20f, ("v", 20f));
        Test("{{ feature[\"v\"] + 21 }}", 41f, ("v", 20f));

        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1f); });
        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1f, ("22", 22f)); });
    }

    [Fact]
    public void FloatNullable()
    {
        Test<float?>("1", 1f);
        Test<float?>("2", 2f);
        Test<float?>("default", null);
        Test<float?>("{{ default }}", null);
        Test<float?>("null", null);
        Test<float?>("{{ null }}", null);
        Test<float?>("", 23f, default, 23f);
        Test<float?>(null, 23f, default, 23f);
        Test<float?>("{{ feature[\"v\"] }}", 20f, ("v", 20f));
        Test<float?>("{{ feature[\"v\"] }}", null, ("22", 22f));
        Test<float?>("{{ feature[\"v\"] + 21 }}", 41f, ("v", 20f));
        Test<float?>("{{ feature[\"v\"] }}", null, ("22", 22f));
        Test<float?>("{{ feature[\"v\"] + 21 }}", null, ("22", 22f));
    }

    [Fact]
    public void Double()
    {
        Test("1", 1d);
        Test("2", 2d);
        Test("default", 0d);
        Test("{{ default }}", 0d);
        Test("", 23d, default, 23d);
        Test(null, 23f, default, 23d);
        Test("{{ feature[\"v\"] }}", 20d, ("v", 20d));
        Test("{{ feature[\"v\"] + 21 }}", 41d, ("v", 20d));

        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1d); });
        Assert.Throws<RuntimeBinderException>(() => { Test("{{ feature[\"v\"] }}", 1d, ("22", 22d)); });
    }

    [Fact]
    public void DoubleNullable()
    {
        var t = typeof(List<string>);

        Test<double?>("1", 1d);
        Test<double?>("2", 2d);
        Test<double?>("default", null);
        Test<double?>("{{ default }}", null);
        Test<double?>("null", null);
        Test<double?>("{{ null }}", null);
        Test<double?>("", 23d, default, 23d);
        Test<double?>(null, 23d, default, 23d);
        Test<double?>("{{ feature[\"v\"] }}", 20d, ("v", 20d));
        Test<double?>("{{ feature[\"v\"] }}", null, ("22", 22d));
        Test<double?>("{{ feature[\"v\"] + 21 }}", 41d, ("v", 20d));
        Test<double?>("{{ feature[\"v\"] }}", null, ("22", 22d));
        Test<double?>("{{ feature[\"v\"] + 21 }}", null, ("22", 22d));

        // 编译为 return (double?)abcd; 执行会出错
        Test<double?>("abcd", null);
    }


    [Fact]
    public void String()
    {
        Test<string>("1", "1");
        Test<string>("2", "2");
        Test<string>("default", "default");
        Test<string>("null", "null");
        Test<string>("{{ null }}", null);
        Test<string>("{{ default }}", null);
        Test<string>("", "23d", default, "23d");
        Test<string>(null, "23d", default, "23d");
        Test<string>("{{ feature[\"v\"] }}", "20d", ("v", "20d"));
        Test<string>("{{ feature[\"v\"] }}", null, ("22", "22d"));
        Test<string>("{{ feature[\"v\"] + 21 }}", "20d21", ("v", "20d"));
        Test<string>("{{ feature[\"v\"] }}", null, ("22", "22d"));
        Test<string>("{{ feature[\"v\"] + 21 }}", null, ("22", "22d"));
    }
}
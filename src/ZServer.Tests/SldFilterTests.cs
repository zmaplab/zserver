using System.Collections.Generic;
using Xunit;
using ZMap.SLD;
using ZMap.SLD.Filter.Visitor;

namespace ZServer.Tests;

public class SldFilterTests
{
    [Fact]
    public void PropertyIsEqualTo()
    {
        var styledLayerDescriptor = StyledLayerDescriptor.Load("sld/PropertyIsEqualTo.xml");
        var namedLayer = (NamedLayer)styledLayerDescriptor.Layers[0];
        var userStyle = (UserStyle)namedLayer.Styles[0];
        var filter = userStyle.FeatureTypeStyles[0].Rules[0].FilterType.Filter;
        var r1 = filter.Accept(new DefaultFilterVisitor(), new Dictionary<string, object>
        {
            { "COMID", "510104024008" }
        });
        Assert.True((bool)r1);
        var r2 = filter.Accept(new DefaultFilterVisitor(), new Dictionary<string, object>
        {
            { "COMID", 510104024008 }
        });
        Assert.True((bool)r2);
        var r3 = filter.Accept(new DefaultFilterVisitor(), new Dictionary<string, object>
        {
            { "COMID", null }
        });
        Assert.False((bool)r3);
        var r4 = filter.Accept(new DefaultFilterVisitor(), null);
        Assert.False((bool)r4);
    }
}
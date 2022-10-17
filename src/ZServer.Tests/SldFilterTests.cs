using Xunit;
using ZMap;
using ZMap.SLD;

namespace ZServer.Tests;

public class SldFilterTests
{
    [Fact]
    public void PropertyIsEqualTo()
    {
        var styledLayerDescriptor = StyledLayerDescriptor.Load("sld/PropertyIsEqualTo.xml");
        styledLayerDescriptor.Accept(new StyleVisitor(), null);
    }
}
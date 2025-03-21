using System.Collections.Generic;
using Xunit;
using ZMap.TileGrid;

namespace ZServer.Tests;

public class SortedListTests
{
    [Fact]
    public void DescendingComparerTest()
    {
        var list1 = new SortedList<double, string>(new DescendingComparer())
        {
            { 1, "one" },
            { 3, "three" },
            { 2, "two" }
        };
        var list2 = new SortedList<double, string>()
        {
            { 1, "one" },
            { 3, "three" },
            { 2, "two" }
        };
        Assert.Equal(list1.Values[0], list2.Values[2]);
    }
}
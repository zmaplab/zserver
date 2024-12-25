using System.Collections.Generic;

namespace ZMap.TileGrid;

public class DescendingComparer : IComparer<double>
{
    public int Compare(double x, double y)
    {
        return Comparer<double>.Default.Compare(y, x);
    }
}
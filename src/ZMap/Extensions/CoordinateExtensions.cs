using NetTopologySuite.Geometries;

namespace ZMap.Extensions
{
    public static class CoordinateExtensions
    {
        public static bool IsEmpty(this Coordinate coordinate)
        {
            if (coordinate == null)
            {
                return true;
            }

            return double.IsNaN(coordinate.X) && double.IsNaN(coordinate.Y);
        }
    }
}
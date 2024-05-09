namespace ZMap.Extensions;

public static class CoordinateExtensions
{
    /// <summary>
    /// 判断坐标是否为空
    /// </summary>
    /// <param name="coordinate"></param>
    /// <returns></returns>
    public static bool IsEmpty(this Coordinate coordinate)
    {
        if (coordinate == null)
        {
            return true;
        }

        return double.IsNaN(coordinate.X) && double.IsNaN(coordinate.Y);
    }
}
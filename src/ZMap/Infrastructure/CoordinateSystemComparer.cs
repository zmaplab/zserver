namespace ZMap.Infrastructure;

using ProjNet.CoordinateSystems;
using System;

public static class CoordinateSystemComparer
{
    // 主比较方法
    public static bool AreEqual(CoordinateSystem cs1, CoordinateSystem cs2)
    {
        if (ReferenceEquals(cs1, cs2)) return true;
        if (cs1 == null || cs2 == null) return false;

        // 1. 类型检查
        if (cs1.GetType() != cs2.GetType()) return false;

        // 2. 名称和授权代码比较
        if (string.Equals(cs1.Name, cs2.Name, StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(cs1.Authority, cs2.Authority, StringComparison.OrdinalIgnoreCase) &&
            cs1.AuthorityCode == cs2.AuthorityCode) return true;

        // 3. 类型分流比较
        if (cs1 is GeographicCoordinateSystem geo1 && cs2 is GeographicCoordinateSystem geo2)
            return AreGeographicEqual(geo1, geo2);

        if (cs1 is ProjectedCoordinateSystem proj1 && cs2 is ProjectedCoordinateSystem proj2)
            return AreProjectedEqual(proj1, proj2);

        return false;
    }

    // 地理坐标系比较
    private static bool AreGeographicEqual(GeographicCoordinateSystem geo1, GeographicCoordinateSystem geo2)
    {
        // 3.1 角度单位
        if (!AreAngularUnitEqual(geo1.AngularUnit, geo2.AngularUnit)) return false;

        // 3.2 本初子午线
        if (!ArePrimeMeridiansEqual(geo1.PrimeMeridian, geo2.PrimeMeridian)) return false;

        // 3.3 水平基准（核心比较）
        return AreHorizontalDatumEqual(geo1.HorizontalDatum, geo2.HorizontalDatum);
    }

    // 投影坐标系比较
    private static bool AreProjectedEqual(ProjectedCoordinateSystem proj1, ProjectedCoordinateSystem proj2)
    {
        // 4.1 基础地理坐标系
        if (!AreEqual(proj1.GeographicCoordinateSystem, proj2.GeographicCoordinateSystem))
            return false;

        // 4.2 线性单位
        if (!AreLinearUnitEqual(proj1.LinearUnit, proj2.LinearUnit)) return false;

        // 4.3 投影方法（关键比较）
        return AreProjectionsEqual(proj1.Projection, proj2.Projection);
    }

    // 水平基准比较（大地基准面核心）
    private static bool AreHorizontalDatumEqual(HorizontalDatum hd1, HorizontalDatum hd2)
    {
        // 基准名称和代码
        if (string.Equals(RemoveEsriPrefix(hd1.Name), RemoveEsriPrefix(hd2.Name),
                StringComparison.OrdinalIgnoreCase)) return true;
        // 要 2 个都有实际值才可以比较
        if (hd1.AuthorityCode > 0 && hd2.AuthorityCode > 0 && hd1.AuthorityCode == hd2.AuthorityCode) return true;

        // 椭球体（核心属性）
        return AreEllipsoidsEqual(hd1.Ellipsoid, hd2.Ellipsoid);
    }

    private static string RemoveEsriPrefix(string name)
    {
        return name.StartsWith("D_") ? name.Substring(2) : name;
    }

    // 椭球体比较
    private static bool AreEllipsoidsEqual(Ellipsoid e1, Ellipsoid e2)
    {
        const double tolerance = 1e-8; // 1毫米精度

        return string.Equals(e1.Name, e2.Name, StringComparison.OrdinalIgnoreCase)
               && Math.Abs(e1.SemiMajorAxis - e2.SemiMajorAxis) < tolerance
               && Math.Abs(e1.InverseFlattening - e2.InverseFlattening) < tolerance
               && Math.Abs(e1.SemiMinorAxis - e2.SemiMinorAxis) < tolerance;
    }

    // 投影方法比较
    private static bool AreProjectionsEqual(IProjection p1, IProjection p2)
    {
        if (string.Equals(p1.Name, p2.Name, StringComparison.OrdinalIgnoreCase)) return true;
        if (p1.NumParameters != p2.NumParameters) return false;

        // 比较所有投影参数
        for (int i = 0; i < p1.NumParameters; i++)
        {
            var param1 = p1.GetParameter(i);
            var param2 = p2.GetParameter(i);

            if (!string.Equals(param1.Name, param2.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (Math.Abs(param1.Value - param2.Value) > 1e-8)
                return false;
        }

        return true;
    }

    // 单位比较（通用方法）
    private static bool AreAngularUnitEqual(AngularUnit u1, AngularUnit u2)
    {
        const double tolerance = 1e-10;
        return string.Equals(u1.Name, u2.Name, StringComparison.OrdinalIgnoreCase)
               && Math.Abs(u1.RadiansPerUnit - u2.RadiansPerUnit) < tolerance;
    }

    private static bool AreLinearUnitEqual(LinearUnit u1, LinearUnit u2)
    {
        const double tolerance = 1e-10;
        return string.Equals(u1.Name, u2.Name, StringComparison.OrdinalIgnoreCase)
               && Math.Abs(u1.MetersPerUnit - u2.MetersPerUnit) < tolerance;
    }


    // 本初子午线比较
    private static bool ArePrimeMeridiansEqual(PrimeMeridian pm1, PrimeMeridian pm2)
    {
        const double radTolerance = 1e-10; // 约0.000002角秒
        return string.Equals(pm1.Name, pm2.Name, StringComparison.OrdinalIgnoreCase)
               && Math.Abs(pm1.Longitude - pm2.Longitude) < radTolerance;
    }
}
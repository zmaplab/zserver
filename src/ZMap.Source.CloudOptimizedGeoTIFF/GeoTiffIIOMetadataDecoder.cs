using System.Reflection;
using ProjNet.CoordinateSystems;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoTiffIIOMetadataDecoder(Dictionary<int, GeoKeyEntry> geoKeys)
{
    private static readonly Dictionary<long, IUnit> UnitCache = new();
    private static readonly Dictionary<string, HorizontalDatum> HorizontalDatumCache = new();
    private static readonly Dictionary<string, PrimeMeridian> PrimeMeridianCache = new();
    private static readonly Dictionary<long, Ellipsoid> EllipsoidCache = new();

    public Dictionary<int, GeoKeyEntry> GeoKeys { get; private set; } = geoKeys;

    static GeoTiffIIOMetadataDecoder()
    {
        typeof(LinearUnit).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(LinearUnit))
            .ToList()
            .ForEach(p =>
            {
                if (p.GetValue(null) is LinearUnit unit)
                {
                    UnitCache.TryAdd(unit.AuthorityCode, unit);
                }
            });

        typeof(AngularUnit).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(AngularUnit))
            .ToList()
            .ForEach(p =>
            {
                if (p.GetValue(null) is AngularUnit unit)
                {
                    UnitCache.TryAdd(unit.AuthorityCode, unit);
                }
            });

        typeof(HorizontalDatum).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(HorizontalDatum))
            .ToList()
            .ForEach(p =>
            {
                if (p.GetValue(null) is HorizontalDatum datum)
                {
                    HorizontalDatumCache.TryAdd($"{datum.Authority}:{datum.AuthorityCode}", datum);
                }
            });

        typeof(PrimeMeridian).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(PrimeMeridian))
            .ToList()
            .ForEach(p =>
            {
                if (p.GetValue(null) is PrimeMeridian pm)
                {
                    PrimeMeridianCache.TryAdd($"{pm.Authority}:{pm.AuthorityCode}", pm);
                }
            });
        typeof(Ellipsoid).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(Ellipsoid))
            .ToList()
            .ForEach(p =>
            {
                if (p.GetValue(null) is Ellipsoid e)
                {
                    EllipsoidCache.TryAdd(e.AuthorityCode, e);
                }
            });
    }

    public int GetModelType()
    {
        return (int)GeoKeys[1024].Value;
    }

    public dynamic GetGeoKey(int key)
    {
        return GeoKeys.TryGetValue(key, out var kv) ? kv.Value : null;
    }

    /// <summary>
    /// 应该有多种来源，除了类中定义的单位，还有如 SQL\CSV 可以提供的单位
    /// </summary>
    /// <param name="unitCode"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool TryGetUnit(long unitCode, out IUnit o)
    {
        return UnitCache.TryGetValue(unitCode, out o);
    }

    public bool TryGetHorizontalDatum(string epsgCode, out HorizontalDatum o)
    {
        return HorizontalDatumCache.TryGetValue(epsgCode, out o);
    }

    public bool TryGetPrimeMeridian(string epsgCode, out PrimeMeridian o)
    {
        return PrimeMeridianCache.TryGetValue(epsgCode, out o);
    }

    public bool TryGetEllipsoid(long epsgCode, out Ellipsoid o)
    {
        return EllipsoidCache.TryGetValue(epsgCode, out o);
    }
}
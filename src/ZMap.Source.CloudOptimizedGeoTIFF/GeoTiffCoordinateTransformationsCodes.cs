namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoTiffCoordinateTransformationsCodes
{
    public static readonly short CT_TransverseMercator = 1;

    public static readonly short CT_TransvMercator_Modified_Alaska = 2;

    public static readonly short CT_ObliqueMercator = 3;

    public static readonly short CT_ObliqueMercator_Laborde = 4;

    public static readonly short CT_ObliqueMercator_Rosenmund = 5;

    public static readonly short CT_ObliqueMercator_Spherical = 6;

    public static readonly short CT_Mercator = 7;

    public static readonly short CT_LambertConfConic_2SP = 8;

    public static readonly short CT_LambertConfConic = CT_LambertConfConic_2SP;

    public static readonly short CT_LambertConfConic_1SP = 9;

    public static readonly short CT_LambertConfConic_Helmert = CT_LambertConfConic_1SP;

    public static readonly short CT_LambertAzimEqualArea = 10;

    public static readonly short CT_AlbersEqualArea = 11;

    public static readonly short CT_AzimuthalEquidistant = 12;

    public static readonly short CT_EquidistantConic = 13;

    public static readonly short CT_Stereographic = 14;

    public static readonly short CT_PolarStereographic = 15;

    public static readonly short CT_ObliqueStereographic = 16;

    public static readonly short CT_Equirectangular = 17;

    public static readonly short CT_CassiniSoldner = 18;

    public static readonly short CT_Gnomonic = 19;

    public static readonly short CT_MillerCylindrical = 20;

    public static readonly short CT_Orthographic = 21;

    public static readonly short CT_Polyconic = 22;

    public static readonly short CT_Robinson = 23;

    public static readonly short CT_Sinusoidal = 24;

    public static readonly short CT_VanDerGrinten = 25;

    public static readonly short CT_NewZealandMapGrid = 26;

    public static readonly short CT_TransvMercator_SouthOriented = 27;

    public static readonly short CT_SouthOrientedGaussConformal = CT_TransvMercator_SouthOriented;

    public static readonly short CT_AlaskaConformal = CT_TransvMercator_Modified_Alaska;

    public static readonly short CT_TransvEquidistCylindrical = CT_CassiniSoldner;

    public static readonly short CT_ObliqueMercator_Hotine = CT_ObliqueMercator;

    public static readonly short CT_SwissObliqueCylindrical = CT_ObliqueMercator_Rosenmund;

    public static readonly short CT_GaussBoaga = CT_TransverseMercator;

    public static readonly short CT_GaussKruger = CT_TransverseMercator;
}
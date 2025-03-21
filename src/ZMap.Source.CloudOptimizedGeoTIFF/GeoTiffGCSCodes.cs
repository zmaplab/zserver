namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoTiffGCSCodes
{
    public static readonly int GCS_NAD27 = 4267;

    public static readonly int GCS_NAD83 = 4269;

    public static readonly int GCS_WGS_72 = 4322;

    public static readonly int GCS_WGS_72BE = 4324;

    public static readonly int GCS_WGS_84 = 4326;

    public static readonly int GCSE_WGS84 = 4030;

    /** An index into the geoKey directory for the directory version number */
    public static readonly int GEO_KEY_DIRECTORY_VERSION_INDEX = 0;

    /** An index into the geoKey directory for the geoKey minor revision number */
    public static readonly int GEO_KEY_MINOR_REVISION_INDEX = 2;

    /** An index into the geoKey directory for the number of geoKeys */
    public static readonly int GEO_KEY_NUM_KEYS_INDEX = 3;

    /** An index into the geoKey directory for the geoKey revision number */
    public static readonly int GEO_KEY_REVISION_INDEX = 1;

    /**
     * GeogAngularUnitsGeoKey Key ID = 2054 Type = SHORT (code) Values = Section 6.3.1.4 Codes
     * Allows the definition of geocentric CS Linear units for user-defined GCS and for ellipsoids.
     * GeoKey Requirements for "user-defined" units: GeogCitationGeoKey GeogAngularUnitSizeGeoKey
     */
    public static readonly int GeogAngularUnitsGeoKey = 2054;

    /**
     * GeogAngularUnitSizeGeoKey Key ID = 2055 Type = DOUBLE Units: radians Allows the definition of
     * user-defined angular geographic units, as measured in radians.
     */
    public static readonly int GeogAngularUnitSizeGeoKey = 2055;

    /**
     * GeogAzimuthUnitsGeoKey Key ID = 2060 Type = SHORT (code) Values = Section 6.3.1.4 Codes This
     * key may be used to specify the angular units of measurement used to defining azimuths, in
     * geographic coordinate systems. These may be used for defining azimuthal parameters for some
     * projection algorithms, and may not necessarily be the same angular units used for lat-long.
     */
    public static readonly int GeogAzimuthUnitsGeoKey = 2060;

    /**
     * GeogCitationGeoKey Key ID = 2049 Type = ASCII Values = text General citation and reference
     * for all Geographic CS parameters.
     */
    public static readonly int GeogCitationGeoKey = 2049;

    /**
     * GeogEllipsoidGeoKey Key ID = 2056 Type = SHORT (code) Values = Section 6.3.2.3 Codes
     * http://www.remotesensing.org/geotiff/spec/geotiff6.html#6.3.2.3 This key may be used to
     * specify the coded ellipsoid used in the geodetic datum of the Geographic Coordinate System.
     * GeoKey Requirements for User-Defined Ellipsoid: GeogCitationGeoKey [GeogSemiMajorAxisGeoKey,
     * [GeogSemiMinorAxisGeoKey | GeogInvFlatteningGeoKey] ]
     */
    public static readonly int GeogEllipsoidGeoKey = 2056;

    /**
     * GeogGeodeticDatumGeoKey Key ID = 2050 Type = SHORT (code) Values = Section 6.3.2.2 Codes
     * http://www.remotesensing.org/geotiff/spec/geotiff6.html#6.3.2.2 This key may be used to
     * specify the horizontal datum, defining the size, position and orientation of the reference
     * ellipsoid used in user-defined geographic coordinate systems. GeoKey Requirements for
     * User-Defined Horizontal Datum: GeogCitationGeoKey GeogEllipsoidGeoKey
     */
    public static readonly int GeogGeodeticDatumGeoKey = 2050;

    /**
     * GeogInvFlatteningGeoKey Key ID = 2059 Type = DOUBLE Units: none. Allows the specification of
     * the inverse of user-defined Ellipsoid's flattening parameter (f). The eccentricity-squared
     * e^2 of the ellipsoid is related to the non-inverted f by: e^2 = 2f - f^2 Note: if the
     * ellipsoid is spherical the inverse-flattening becomes infinite; use the
     * GeogSemiMinorAxisGeoKey instead, and set it equal to the semi-major axis length.
     */
    public static readonly int GeogInvFlatteningGeoKey = 2059;

    /**
     * GeogLinearUnitsGeoKey Key ID = 2052 Type = SHORT Values: Section 6.3.1.3 Codes
     * http://www.remotesensing.org/geotiff/spec/geotiff6.html#6.3.1.3 Allows the definition of
     * geocentric CS linear units for user-defined GCS.
     */
    public static readonly int GeogLinearUnitsGeoKey = 2052;

    /**
     * GeogLinearUnitSizeGeoKey Key ID = 2053 Type = DOUBLE Units: meters Allows the definition of
     * user-defined linear geocentric units, as measured in meters.
     */
    public static readonly int GeogLinearUnitSizeGeoKey = 2053;

    /**
     * GeogPrimeMeridianGeoKey Key ID = 2051 Type = SHORT (code) Units: Section 6.3.2.4 code
     * http://www.remotesensing.org/geotiff/spec/geotiff6.html#6.3.2.4 Allows specification of the
     * location of the Prime meridian for user-defined geographic coordinate systems. The default
     * standard is Greenwich, England.
     */
    public static readonly int GeogPrimeMeridianGeoKey = 2051;

    /**
     * GeogPrimeMeridianLongGeoKey Key ID = 2061 Type = DOUBLE Units = GeogAngularUnits This key
     * allows definition of user-defined Prime Meridians, the location of which is defined by its
     * longitude relative to Greenwich.
     */
    public static readonly int GeogPrimeMeridianLongGeoKey = 2061;

    /**
     * GeographicTypeGeoKey Key ID = 2048 Type = SHORT (code) Values = Section 6.3.2.1 Codes
     * http://www.remotesensing.org/geotiff/spec/geotiff6.html#6.3.2.1 This key may be used to
     * specify the code for the geographic coordinate system used to map lat-long to a specific
     * ellipsoid over the earth. GeoKey Requirements for User-Defined geographic CS:
     * GeogCitationGeoKey GeogGeodeticDatumGeoKey GeogAngularUnitsGeoKey (if not degrees)
     * GeogPrimeMeridianGeoKey (if not Greenwich)
     */
    public static readonly int GeographicTypeGeoKey = 2048;

    /**
     * GeogSemiMajorAxisGeoKey Key ID = 2057 Type = DOUBLE Units: Geocentric CS Linear Units Allows
     * the specification of user-defined Ellipsoid Semi-Major Axis (a).
     */
    public static readonly int GeogSemiMajorAxisGeoKey = 2057;

    /**
     * GeogSemiMinorAxisGeoKey Key ID = 2058 Type = DOUBLE Units: Geocentric CS Linear Units Allows
     * the specification of user-defined Ellipsoid Semi-Minor Axis (b).
     */
    public static readonly int GeogSemiMinorAxisGeoKey = 2058;

    /**
     * GTCitationGeoKey Key ID = 1026 Type = ASCII As with all the "Citation" GeoKeys, this is
     * provided to give an ASCII reference to published documentation on the overall configuration
     * of this GeoTIFFWritingUtilities file.
     */
    public static readonly int GTCitationGeoKey = 1026;

    public static readonly int ModelTypeGeocentric = 3;

    public static readonly int ModelTypeGeographic = 2;
}
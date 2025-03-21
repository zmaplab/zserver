namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class GeoTiffConstants
{
    public static readonly int TIFFTAG_NODATA = 42113;

    public static readonly int GTUserDefinedGeoKey = 32767;

    public static readonly string GTUserDefinedGeoKey_String = "32767";

    public static readonly short ARRAY_ELEM_INCREMENT = 5;

    // public static readonly string GEOTIFF_IIO_METADATA_FORMAT_NAME =
    //     it.geosolutions.imageioimpl.plugins.tiff.TIFFImageMetadata.nativeMetadataFormatName;

    // public static readonly string GEOTIFF_IIO_ROOT_ELEMENT_NAME = GEOTIFF_IIO_METADATA_FORMAT_NAME;

    public static readonly int USHORT_MAX = (1 << 16) - 1;

    public static readonly int USHORT_MIN = 0;

    /**
     * GTModelTypeGeoKey Key ID = 1024 Type: SHORT (code) Values: Section 6.3.1.1 Codes This GeoKey
     * defines the general type of model Coordinate system used, and to which the raster space will
     * be transformed: unknown, Geocentric (rarely used), Geographic, Projected Coordinate System,
     * or user-defined. If the coordinate system is a PCS, then only the PCS code need be specified.
     * If the coordinate system does not fit into one of the standard registered PCS'S, but it uses
     * one of the standard projections and datums, then its should be documented as a PCS model with
     * "user-defined" type, requiring the specification of projection parameters, etc. GeoKey
     * requirements for User-Defined Model Type (not advisable): GTCitationGeoKey
     */
    public static readonly int GTModelTypeGeoKey = 1024;

    /**
     * GTRasterTypeGeoKey Key ID = 1025 Type = Section 6.3.1.2 codes This establishes the Raster
     * Space coordinate system used; there are currently only two, namely RasterPixelIsPoint and
     * RasterPixelIsArea.
     */
    public static readonly int GTRasterTypeGeoKey = 1025;

    /**
     * 6.3.1.2 Raster Type Codes Ranges: 0 = undefined [ 1, 1023] = Raster Type Codes
     * (GeoTIFFWritingUtilities Defined) [1024, 32766] = Reserved 32767 = user-defined [32768,
     * 65535]= Private User
     */
    public static readonly int RasterPixelIsArea = 1;

    public static readonly int RasterPixelIsPoint = 2;

    /** The DOM element ID (tag) for a single TIFF Ascii value */
    public static readonly string GEOTIFF_ASCII_TAG = "TIFFAscii";

    /** The DOM element ID (tag) for a set of TIFF Ascii values */
    public static readonly string GEOTIFF_ASCIIS_TAG = "TIFFAsciis";

    /**
     * The DOM element ID (tag) for a single TIFF double. The value is stored in an attribute named
     * "value"
     */
    public static readonly string GEOTIFF_DOUBLE_TAG = "TIFFDouble";

    /** The DOM element ID (tag) for a set of TIFF Double values */
    public static readonly string GEOTIFF_DOUBLES_TAG = "TIFFDoubles";

    /** The DOM element ID (tag) for a TIFF Field */
    public static readonly string GEOTIFF_FIELD_TAG = "TIFFField";

    /** The DOM element ID (tag) for a TIFF Image File Directory */
    public static readonly string GEOTIFF_IFD_TAG = "TIFFIFD";

    /**
     * The DOM element ID (tag) for a single TIFF Short value. The value is stored in an attribute
     * named "value"
     */
    public static readonly string GEOTIFF_SHORT_TAG = "TIFFShort";

    /** The DOM element ID (tag) for a set of TIFF Short values */
    public static readonly string GEOTIFF_SHORTS_TAG = "TIFFShorts";

    /** The DOM attribute name for a TIFF Entry value (whether Short, Double, or Ascii) */
    public static readonly string VALUE_ATTRIBUTE = "value";

    /** The DOM attribute name for a TIFF Field Tag (number) */
    public static readonly string NUMBER_ATTRIBUTE = "number";

    /** The DOM attribute name for a TIFF Field Tag (name) */
    public static readonly string NAME_ATTRIBUTE = "name";

    public static readonly string GEOTIFF_TAGSETS_ATT_NAME = "tagSets";

    public static readonly int DEFAULT_GEOTIFF_VERSION = 1;

    public static readonly int DEFAULT_KEY_REVISION_MAJOR = 1;

    public static readonly int DEFAULT_KEY_REVISION_MINOR = 2;

    public static readonly int UNDEFINED = 0;
}
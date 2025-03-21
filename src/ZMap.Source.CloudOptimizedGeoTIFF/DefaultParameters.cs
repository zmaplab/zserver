using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Projections;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class DefaultParameters
{
    public static ProjectionParameterSet CreateTransverseMercator()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // CENTRAL_MERIDIAN,
        // LATITUDE_OF_ORIGIN,
        // SCALE_FACTOR,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("central", 0),
            new("latitude_origin", 0),
            new("scale_factor", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet CreateEquidistantCylindrical()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // CENTRAL_MERIDIAN,
        // LATITUDE_OF_ORIGIN,
        // STANDARD_PARALLEL_1,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("central", 0),
            new("latitude_origin", 0),
            new("standard_parallel_1", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet CreateMercator_1SP()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // LATITUDE_OF_ORIGIN,
        // CENTRAL_MERIDIAN,
        // SCALE_FACTOR,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("latitude_origin", 0),
            new("central", 0),
            new("scale_factor", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet CreateMercator_2SP()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // STANDARD_PARALLEL_1,
        // LATITUDE_OF_ORIGIN,
        // CENTRAL_MERIDIAN,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("standard_parallel_1", 0),
            new("latitude_origin", 0),
            new("central", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet lambert_conformal_conic_1SP()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // CENTRAL_MERIDIAN,
        // LATITUDE_OF_ORIGIN,
        // SCALE_FACTOR,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("central", 0),
            new("latitude_origin", 0),
            new("scale_factor", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet lambert_conformal_conic_2SP()
    {
        // SEMI_MAJOR, 
        // SEMI_MINOR,
        // CENTRAL_MERIDIAN,
        // LATITUDE_OF_ORIGIN,
        // STANDARD_PARALLEL_1,
        // STANDARD_PARALLEL_2,
        // FALSE_EASTING, 
        // FALSE_NORTHING,
        // SCALE_FACTOR
        // // This last parameter is for backwards compatibility, see
        // // LambertConformalEsriProvider
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("central", 0),
            new("standard_parallel_1", 0),
            new("standard_parallel_2", 0),
            new("latitude_origin", 0),
            new("false_easting", 0),
            new("false_northing", 0),
            new("scale_factor", 0)
        });
    }

    public static ProjectionParameterSet Krovak()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // LATITUDE_OF_CENTER,
        // LONGITUDE_OF_CENTER,
        // AZIMUTH,
        // PSEUDO_STANDARD_PARALLEL,
        // SCALE_FACTOR,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("latitude_center", 0),
            new("longitude_center", 0),
            new("azimuth", 0),
            new("pseudo_standard_parallel", 0),
            new("scale_factor", 0),
            new("false_easting", 0),
            new("false_northing", 0),
        });
    }

    public static ProjectionParameterSet Stereographic()
    {
        // SEMI_MAJOR,
        // SEMI_MINOR,
        // CENTRAL_MERIDIAN,
        // LATITUDE_OF_ORIGIN,
        // SCALE_FACTOR,
        // FALSE_EASTING,
        // FALSE_NORTHING
        return new ProjectionParameterSet(new List<ProjectionParameter>
        {
            new("semi_major", 0),
            new("semi_minor", 0),
            new("central", 0),
            new("latitude_origin", 0),
            new("scale_factor", 0),
            new("false_easting", 0),
            new("false_northing", 0)
        });
    }

    public static ProjectionParameterSet polar_stereographic()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Oblique_Stereographic()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet oblique_mercator()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Albers_Conic_Equal_Area()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Orthographic()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Lambert_Azimuthal_Equal_Area()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Azimuthal_Equidistant()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet New_Zealand_Map_Grid()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet World_Van_der_Grinten_I()
    {
        throw new NotImplementedException();
    }

    public static ProjectionParameterSet Sinusoidal()
    {
        throw new NotImplementedException();
    }
}
using NetTopologySuite.Features;

namespace ZMap.Ogc.Wms;

public record GetFeatureInfoResult(FeatureCollection Features, string Code, string Message);
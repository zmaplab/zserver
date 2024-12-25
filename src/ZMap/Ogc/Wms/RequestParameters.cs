namespace ZMap.Ogc.Wms;

public record RequestParameters(
    Envelope Envelope,
    int SRID,
    IReadOnlyCollection<(string ResourceGroup, string Layer)> Layers,
    string[] Styles,
    string[] Filters);
namespace ZMap.Ogc;

public record LayerQuery(
    string ResourceGroup,
    string Layer,
    string Style);
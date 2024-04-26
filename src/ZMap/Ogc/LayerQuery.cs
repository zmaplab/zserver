using System.Collections.Generic;

namespace ZMap.Ogc;

public record LayerQuery(
    string ResourceGroup,
    string Layer,
    string Style,
    IDictionary<string, object> Arguments = null);
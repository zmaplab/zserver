using System.Collections.Generic;

namespace ZMap.Ogc;

public record LayerQuery(
    string ResourceGroup,
    string Layer,
    IDictionary<string, object> Arguments = null);
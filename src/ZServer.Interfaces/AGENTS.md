# ZServer.Interfaces - Orleans Grain Contracts

## OVERVIEW
Orleans grain interfaces defining the distributed actor contracts for WMS, WMTS, XYZ, and RESTful operations. 11 C# files. Public API surface for grain interactions.

## STRUCTURE
```
ZServer.Interfaces/
├── WMS/IWMSGrain.cs             # WMS grain: GetCapabilities, GetMap, GetFeatureInfo
├── WMTS/IWMTSGrain.cs           # WMTS grain contract
├── XYZ/IXyzGrain.cs             # XYZ tile grain (z/x/y)
├── RESTFUL/IRestful.cs          # RESTful data operations grain
├── Obsolete/                    # Deprecated grain interfaces
├── ZServerResponse.cs           # Unified grain response wrapper
├── ZServerResponseFactory.cs    # Response factory helpers
├── ServerException.cs           # Grain exception types
└── ServerExceptionReport.cs     # OGC exception report model
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Add OGC operation to WMS | `WMS/IWMSGrain.cs` | Add `ValueTask<ZServerResponse>` method with OGC params |
| Add new grain type | New `I*Grain.cs` | Must extend `IGrainWithIntegerKey` or `IGrainWithStringKey` |
| Change response format | `ZServerResponse.cs` + `ZServerResponseFactory.cs` | All grains return `ZServerResponse` |
| Handle grain errors | `ServerException.cs` | Throw from grain impl, caught by API filters |

## INTERNAL PATTERNS
- **Grain keying**: All grains use integer keys via `IGrainWithIntegerKey` (maps to layer hash)
- **Response model**: Every grain method returns `ValueTask<ZServerResponse>` — typed wrapper with status, body, content-type
- **OGC parameters**: Passed as individual params (layers, srs, bbox, width, height, format) not DTOs
- **Error propagation**: `ServerException` with OGC exception codes → API layer converts to XML/JSON error
- **Obsolete interfaces**: Moved to `Obsolete/` dir — do NOT reference in new code

## NOTES
- Grain interface changes require re-deploying all silos (Orleans interface versioning)
- `IZServerResponseFactory` provides static factory methods for common response types
- `IWMSGrain.GetMapAsync` has 13 parameters — keep param list stable, add new params at end with defaults

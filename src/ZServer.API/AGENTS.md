# ZServer.API - Web API Host

## OVERVIEW
ASP.NET Core host providing OGC WMS/WMTS/XYZ over HTTP. Forwards requests to Orleans grains for processing. 25 C# files.

## STRUCTURE
```
ZServer.API/
├── Program.cs                   # Main entry (Orleans silo+client bootstrap, middleware pipeline)
├── Controllers/
│   ├── WMSController.cs         # /wms endpoints → IWMSGrain
│   ├── WMTSController.cs        # /wmts endpoints → IWMTSGrain
│   ├── XyzController.cs         # /xyz/{z}/{x}/{y} tile endpoints
│   └── ToolControler.cs         # Utility endpoints
├── Middlewares/                  # Custom HTTP middleware
├── Filters/                     # GlobalExceptionFilter, action filters
├── Features/                    # Custom features (TraceIdentifierFeature)
├── Authentication/              # JWT + scope-based auth handlers
├── Permission/                  # PermissionService (scoped access)
├── conf/                        # appsettings.json, serilog.json
└── JwtBearerAuthenticationExtensions.cs  # JWT setup with scope-based policies
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Add API endpoint | `Controllers/` | Controller forwards to Orleans grain, returns `ZServerResponse` |
| Add middleware | `Middlewares/` | Insert into pipeline in `Program.cs` |
| Add global filter | `Filters/` | Register in `AddControllers(x => x.Filters.Add(...))` |
| Change auth behavior | `JwtBearerAuthenticationExtensions.cs` | JWT scope policy: "default" requires scope=ApiName |
| Change health checks | `Program.cs` | Health check at `/healthz` |
| Change CORS policy | `Program.cs` `AddCors` call | `CrosPolicy` variable |

## INTERNAL PATTERNS
- **Request flow**: Controller → Orleans grain (IWMSGrain/IWMTSGrain) → ZServerResponse → JSON
- **Auth**: JWT Bearer with scope-based policy. `EnableAuthorization` config toggle. When disabled, `RequireAssertion(_ => true)` allows all
- **Error handling**: `GlobalExceptionFilter` catches unhandled exceptions
- **Single-file mode**: `ExcludeFromSingleFile=true` for runtime data (fonts, shapes, sld)
- **Configuration**: `conf/appsettings.json` + `ZSERVER_` env var prefix
- **Orleans co-hosting**: `builder.Host.ConfigureSilo()` in Program.cs — both silo and client in same process

## NOTES
- `Startup.cs` is legacy — most initialization moved to `Program.cs` WebApplication builder pattern
- Kestrel configured with 1GB max request body for large tile responses
- OpenTelemetry wired via `builder.AddOtel(apiName)`

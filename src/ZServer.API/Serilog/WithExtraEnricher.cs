using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace ZServer.API.Serilog;

/// <summary>
/// 
/// </summary>
public class WithExtraEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///
    /// </summary>
    public WithExtraEnricher()
    {
        _httpContextAccessor = new HttpContextAccessor();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        var request = context.Request;
        var headers = request.Headers;
        var ip = GetRemoteIpAddressString(context);
        AddScalarProperty(logEvent, "ip", ip);

        AddScalarProperty(logEvent, "request_method", request.Method);
        AddScalarProperty(logEvent, "query_string", request.QueryString.Value);

        // userinfo
        AddHeaderProperty(logEvent, headers, "user_id", "z-user-id");
        AddHeaderProperty(logEvent, headers, "user_name", "z-user-name");

        AddHeaderProperty(logEvent, headers, "application_id", "z-application-id");
        AddHeaderProperty(logEvent, headers, "device_id", "z-device-id");
        AddHeaderProperty(logEvent, headers, "os", "z-os");
        AddHeaderProperty(logEvent, headers, "app_id", "z-app-id");
        AddHeaderProperty(logEvent, headers, "imei", "z-imei");
        AddHeaderProperty(logEvent, headers, "alt", "z-alt");
        AddHeaderProperty(logEvent, headers, "lat", "z-lat");
        AddHeaderProperty(logEvent, headers, "lon", "z-lon");
        AddHeaderProperty(logEvent, headers, "platform", "z-platform");
        logEvent.AddOrUpdateProperty(new LogEventProperty("uri", new ScalarValue(request.Path)));

        AddHeaderProperty(logEvent, headers, "protocol", request.Protocol);

        logEvent.RemovePropertyIfPresent("RequestPath");
        logEvent.RemovePropertyIfPresent("ConnectionId");
        logEvent.RemovePropertyIfPresent("ActionId");
        logEvent.RemovePropertyIfPresent("RequestId");
    }

    private void AddScalarProperty(LogEvent logEvent, string propertyName, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(new LogEventProperty(propertyName, new ScalarValue(value)));
    }

    private void AddHeaderProperty(LogEvent logEvent, IHeaderDictionary headers, string propertyName,
        params string[] headerNames)
    {
        foreach (var headerName in headerNames)
        {
            if (!headers.ContainsKey(headerName))
            {
                continue;
            }

            var value = headers[headerName].ToString();
            if (!string.IsNullOrEmpty(value))
            {
                logEvent.AddPropertyIfAbsent(
                    new LogEventProperty(propertyName, new ScalarValue(value)));
                break;
            }
        }
    }

    private static string GetRemoteIpAddressString(HttpContext context)
    {
        var remoteIpAddressString = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(remoteIpAddressString))
            remoteIpAddressString = context.Connection.RemoteIpAddress?.ToString();
        return remoteIpAddressString;
    }
}
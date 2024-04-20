using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace ZServer.API;

public class WithExtraEnricher : ILogEventEnricher
{
    private readonly HttpContext _httpContext;

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpContext"></param>
    public WithExtraEnricher(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_httpContext == null)
        {
            return;
        }

        var ip = GetRemoteIpAddressString(_httpContext);
        logEvent.AddPropertyIfAbsent(new LogEventProperty("client_ip", new ScalarValue(ip)));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("user_id",
            new ScalarValue(_httpContext.Request.Headers["z-user-id"].ToString())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("user_name",
            new ScalarValue(_httpContext.Request.Headers["z-user-name"].ToString())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("trace_id",
            new ScalarValue(_httpContext.Request.Headers["trace-id"].ToString())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("frontend_version",
            new ScalarValue(_httpContext.Request.Headers["x-frontend-version"].ToString())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("application_id",
            new ScalarValue(_httpContext.Request.Headers["z-application-id"].ToString())));
    }

    private static string GetRemoteIpAddressString(HttpContext context)
    {
        var remoteIpAddressString = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(remoteIpAddressString))
            remoteIpAddressString = context.Connection.RemoteIpAddress?.ToString();
        return remoteIpAddressString;
    }
}
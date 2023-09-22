using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using ZMap;

namespace ZServer.API;

public class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggerMiddleware> _logger;

    public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var hostIp = EnvironmentVariables.HostIP;
            if (!string.IsNullOrWhiteSpace(hostIp))
            {
                LogContext.PushProperty("HOST_IP", hostIp); //traceId
            }

            if (context.Request.Headers.TryGetValue("trace-id", out var header))
            {
                LogContext.PushProperty("TraceId", header.ToString()); //traceId
            }
            else
            {
                context.Request.Headers.Add("trace-id", context.TraceIdentifier);
                LogContext.PushProperty("TraceId", context.TraceIdentifier); //traceId
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Log middleware error");
        }

        await _next(context);
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace ZServer.API.Middlewares;

/// <summary>
/// 
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="logger"></param>
public class HttpContextFactory(IServiceProvider serviceProvider, ILogger<HttpContextFactory> logger)
    : IHttpContextFactory
{
    private readonly DefaultHttpContextFactory _defaultHttpContextFactory = new(serviceProvider);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="featureCollection"></param>
    /// <returns></returns>
    public HttpContext Create(IFeatureCollection featureCollection)
    {
        var httpContext = _defaultHttpContextFactory.Create(featureCollection);
        // OpenTel 标准，不需要做额外操作
        if (!httpContext.Request.Headers.ContainsKey("traceparent"))
        {
            string traceId = null;
            try
            {
                traceId = GetHeaderValue(httpContext.Request.Headers, "trace-id", "z-trace-id", "traceid",
                    "X-Trace-Id", "X-Request-Id");
                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    traceId = traceId.Replace("-", "");
                    if (traceId.Length == 32 && IsLowerCaseHexAndNotAllZeros(traceId))
                    {
                        httpContext.TraceIdentifier = traceId;
                        httpContext.Request.Headers.TryAdd("traceparent", $"00-{traceId}-ffffffffffffffff-00");
                        logger.LogDebug("Overwrite trace id {TraceIdentifier}", traceId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "解析 TraceId 失败: {TraceId}", traceId);
            }
        }

        return httpContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHexLowerChar(int c)
    {
        return (uint)(c - '0') <= 9 || (uint)(c - 'a') <= 'f' - 'a';
    }

    private static bool IsLowerCaseHexAndNotAllZeros(ReadOnlySpan<char> idData)
    {
        // Verify lower-case hex and not all zeros https://w3c.github.io/trace-context/#field-value
        var isNonZero = false;
        var i = 0;
        for (; i < idData.Length; i++)
        {
            var c = idData[i];
            if (!IsHexLowerChar(c))
            {
                return false;
            }

            if (c != '0')
            {
                isNonZero = true;
            }
        }

        return isNonZero;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    public void Dispose(HttpContext httpContext)
    {
        _defaultHttpContextFactory.Dispose(httpContext);
    }

    private static string GetHeaderValue(IHeaderDictionary dict, params string[] headers)
    {
        foreach (var header in headers)
        {
            if (dict.TryGetValue(header, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
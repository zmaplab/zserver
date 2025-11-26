using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ZServer.API.Features;

/// <summary>
/// 自定义请求标识 Feature，优先从 Header 读取 TraceIdentifier
/// </summary>
public class TraceIdentifierFeature(
    IHttpRequestIdentifierFeature originalFeature,
    IHttpContextAccessor httpContextAccessor)
    : IHttpRequestIdentifierFeature
{
    // 原始 Feature（用于 fallback 自动生成）
    private readonly IHttpRequestIdentifierFeature _originalFeature =
        originalFeature ?? throw new ArgumentNullException(nameof(originalFeature));

    /// <summary>
    /// 核心：优先从 Header 取，无则用原始 Feature 的自动生成值
    /// </summary>
    public string TraceIdentifier
    {
        get
        {
            // 从 HttpContext 读取自定义 Header（需通过 IHttpContextAccessor 获取当前上下文）
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var traceId = GetHeaderValue(httpContext.Request.Headers, "trace-id", "z-trace-id", "traceparent",
                    "X-Trace-Id", "X-Request-Id");
                if (!string.IsNullOrEmpty(traceId))
                {
                    return traceId;
                }
            }

            // 无 Header 时，返回框架自动生成的值
            return _originalFeature.TraceIdentifier;
        }
        set => _originalFeature.TraceIdentifier = value; // 保留设置能力（可选）
    }

    // 注入 IHttpContextAccessor 以获取当前请求上下文
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

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
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ZServer.API.Middlewares;

/// <summary>
/// 
/// </summary>
public static class HealthCheckUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static HealthCheckOptions CreateHealthCheckOptions()
    {
        var healthCheckOptions = new HealthCheckOptions
        {
            ResponseWriter = async (context, result) =>
            {
                // 构建自定义响应内容
                var response = new
                {
                    status = result.Status.ToString(),
                    services = result.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds,
                        error = e.Value.Exception?.Message
                    }),
                    timestamp = DateTime.UtcNow
                };
                // 设置响应类型和状态码
                context.Response.ContentType = "plain/text";
                context.Response.StatusCode = result.Status == HealthStatus.Healthy
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status503ServiceUnavailable;
                // 序列化并返回
                await JsonSerializer.SerializeAsync(context.Response.Body, response);
            }
        };

        return healthCheckOptions;
    }
}
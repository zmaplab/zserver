using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using ZServer.API.Serilog;
using ExportProcessorType = OpenTelemetry.ExportProcessorType;

namespace ZServer.API;

/// <summary>
/// 
/// </summary>
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WebApplicationBuilder AddSerilog()
        {
            if (File.Exists("conf/serilog.json"))
            {
                builder.Configuration.AddJsonFile("conf/serilog.json", optional: true, reloadOnChange: false);
            }

            var serilogSection = builder.Configuration.GetSection("Serilog");
            if (serilogSection.GetChildren().Any())
            {
                Log.Logger = new LoggerConfiguration().ReadFrom
                    .Configuration(builder.Configuration).Enrich.With(new WithExtraEnricher())
                    .CreateLogger();
            }
            else
            {
                var logFile = builder.Configuration["LOG_PATH"] ?? builder.Configuration["LOGPATH"];
                if (string.IsNullOrEmpty(logFile))
                {
                    logFile = Environment.GetEnvironmentVariable("LOG");
                }

                if (string.IsNullOrEmpty(logFile))
                {
                    logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/log.txt");
                }

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    // Serilog.Enrichers.Thread
                    .Enrich.WithThreadId()
                    // Serilog.Enrichers.Environment
                    .Enrich.WithMachineName()
                    .WriteTo.Console()
                    // .WriteTo.ClickHouse()
                    .WriteTo.Async(x => x.File(logFile, rollingInterval: RollingInterval.Day))
                    .CreateLogger();
            }

            return builder;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public WebApplicationBuilder AddOtel(string serviceName)
        {
            var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                var apiKey = builder.Configuration["OpenTelemetry:ApiKey"];
                var authorization = string.IsNullOrEmpty(apiKey) ? null : $"Authorization={apiKey}";
                var samplerProbability = builder.Configuration.GetSection("OpenTelemetry")
                    .GetValue<double?>("SamplerProbability") ?? 0.5;
                var instanceId =
                    $"{Environment.GetEnvironmentVariable("DAPR_HOST_IP") ?? Environment.GetEnvironmentVariable("HOST_IP")}:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}";
                instanceId = string.IsNullOrWhiteSpace(instanceId) ? null : instanceId;
                var @namespace = builder.Configuration["OpenTelemetry:Namespace"];
                // 2. 添加 OpenTelemetry 服务
                builder.Services.AddOpenTelemetry()
                    .ConfigureResource(configure =>
                    {
                        configure.AddService(
                                serviceName: serviceName, // 替换为你的服务名
                                serviceVersion: "1.0.0", serviceInstanceId: instanceId, serviceNamespace: @namespace,
                                autoGenerateServiceInstanceId: true)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["deployment.environment"] = builder.Environment.EnvironmentName
                            });
                    })
                    // 🔍 追踪（Traces）配置
                    .WithTracing(tracing => tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true; // 记录请求异常
                            // 过滤健康检查等端点
                            options.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                        })
                        .AddHttpClientInstrumentation() // 追踪 HttpClient 调用
                        .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplerProbability))) // 采样率
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.ExportProcessorType = ExportProcessorType.Batch; // 批量导出提升性能
                            // 👇 关键：添加 Authorization Header
                            options.Headers = authorization;
                            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        }))
                    // 📊 指标（Metrics）配置
                    .WithMetrics(metrics => metrics
                        .AddAspNetCoreInstrumentation() // ASP.NET Core 内置指标
                        .AddRuntimeInstrumentation() // 运行时指标（CPU、内存、GC）
                        .AddMeter("Microsoft.AspNetCore.Hosting") // Kestrel 指标
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            // 👇 关键：添加 Authorization Header
                            options.Headers = authorization;
                            options.ExportProcessorType = ExportProcessorType.Batch; // 批量导出提升性能
                            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        }))
                    // 📝 日志（Logs）配置
                    // 统一由 Serilog 管理
                    // .WithLogging(
                    //     logging =>
                    //     {
                    //         logging.AddOtlpExporter(options =>
                    //         {
                    //             options.Endpoint = new Uri(otlpEndpoint);
                    //             // 👇 关键：添加 Authorization Header
                    //             options.Headers = authorization;
                    //         });
                    //     },
                    //     configure =>
                    //     {
                    //         configure.IncludeFormattedMessage = true;
                    //         configure.IncludeScopes = true;
                    //         configure.SetResourceBuilder(appResourceBuilder);
                    //         configure.AddProcessor(new LogRecordProcessor());
                    //     })
                    ;
            }

            return builder;
        }
    }
}
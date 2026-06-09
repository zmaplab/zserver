using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.Converters;
using Orleans.Configuration;
using Serilog;
using ZMap;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
using ZMap.Permission;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.API.Authentication;
using ZServer.API.Features;
using ZServer.API.Filters;
using ZServer.API.Middlewares;
using ZServer.API.Permission;
using ZServer.Silo;

#if !DEBUG
#endif

namespace ZServer.API;

/// <summary>
/// 
/// </summary>
public class Program
{
    private static readonly string CrosPolicy = "___my_cors";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        Utility.PrintInfo();

        // FixOrleansPublishSingleFileIssue();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        NtsGeometryServices.Instance = new NtsGeometryServices(
            CoordinateArraySequenceFactory.Instance,
            PrecisionModel.Floating.Value,
            4326, GeometryOverlay.Legacy, new CoordinateEqualityComparer());

        CSharpDynamicCompiler.Load<NatashaDynamicCompiler>();
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        if (!Directory.Exists("cache"))
        {
            Directory.CreateDirectory("cache");
        }

        var app = CreateApp(args);

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        ZMap.Infrastructure.Log.SetLoggerFactory(loggerFactory);
        FontUtility.Load();

        app.UseHealthChecks("/healthz");
        // app.UseResponseCompression();
        app.UseResponseCaching();

        app.UseRouting();
        app.UseCors(CrosPolicy);

        var healthCheckPath = Environment.GetEnvironmentVariable("HEALTH_CHECK_PATH") ?? "/healthz";
        app.UseHealthChecks(healthCheckPath, HealthCheckUtils.CreateHealthCheckOptions());

// 先认证
        app.UseAuthentication();
// 后授权
        app.UseAuthorization();
        app.UseCloudEvents();
        app.MapSubscribeHandler();
        app.MapControllers()
            .RequireCors(CrosPolicy);
        await app.RunAsync();
    }

    private static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel((_, options) =>
        {
            // Handle requests up to 500 MB
            options.Limits.MaxRequestBodySize = 1024288000;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(20);
        });

        if (File.Exists("conf/appsettings.json"))
        {
            builder.Configuration.AddJsonFile("conf/appsettings.json", optional: true, reloadOnChange: true);
        }

        builder.Configuration.AddEnvironmentVariables("ZSERVER_");

        EnvironmentVariables.OrleansHostIP =
            EnvironmentVariables.GetValue(builder.Configuration, "HOST_IP", "HostIP");

        builder.AddSerilog();
        builder.Host.UseSerilog();
        builder.Host.ConfigureSilo();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOpenApi();
        }

        var services = builder.Services;
        // 可选：启用OpenTelemetry追踪（如需可视化/导出TraceId）
        // 替换默认的 IHttpContextFactory, 创建后立即修改 启用OpenTelemetry 相关 Header 才能启作用
        services.AddSingleton<IHttpContextFactory, HttpContextFactory>();

        // 替换默认的 IHttpRequestIdentifierFeature
        services.AddTransient<IHttpRequestIdentifierFeature>(sp =>
        {
            // 获取原始 Feature（框架默认实现）
            var originalFeature = sp.GetRequiredService<HttpRequestIdentifierFeature>();
            // 注入依赖项
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            // 返回自定义 Feature
            return new TraceIdentifierFeature(originalFeature, httpContextAccessor);
        });

        services.AddControllers(x => { x.Filters.Add<GlobalExceptionFilter>(); })
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory()); })
            .AddDapr();
        services.AddResponseCaching();
        services.AddRouting(x => { x.LowercaseUrls = true; });
        services.AddZServer(builder.Configuration).AddSkiaSharpRenderer();
        services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });
        services.Configure<ServerOptions>(builder.Configuration);
        services.Configure<ClusterOptions>("Orleans", builder.Configuration);
        services.AddCors(option =>
        {
            option.AddPolicy(CrosPolicy, policy =>
                policy.AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .WithExposedHeaders("x-suggested-filename")
                    .AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
            );
        });
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddHealthChecks();
        services.Configure<PermissionOptions>(builder.Configuration);
        services.TryAddSingleton<IPermissionService, PermissionService>();

        var apiName = builder.Configuration["ApiName"];
        if (string.IsNullOrWhiteSpace(apiName))
        {
            apiName = "zserver-api";
        }

        builder.AddOtel(apiName);
        builder.AddApiAuthentication(apiName);

        return builder.Build();
    }

    // /// <summary>
    // /// 配置响应顺序，按从低到高：环境 -> 配置 -> command parameters
    // /// </summary>
    // /// <param name="args"></param>
    // /// <returns></returns>
    // private static IHostBuilder CreateHostBuilder(string[] args) =>
    //     Host.CreateDefaultBuilder(args)
    //         .ConfigureHostConfiguration(x =>
    //         {
    //             x.AddEnvironmentVariables();
    //             x.AddCommandLine(args);
    //         })
    //         .ConfigureAppConfiguration((_, builder) =>
    //         {
    //             builder.AddEnvironmentVariables();
    //
    //             if (File.Exists("conf/serilog.json"))
    //             {
    //                 builder.AddJsonFile("conf/serilog.json", optional: true, reloadOnChange: true);
    //             }
    //
    //             if (File.Exists("conf/appsettings.json"))
    //             {
    //                 builder.AddJsonFile("conf/appsettings.json", optional: true, reloadOnChange: true);
    //             }
    //
    //             var configuration = builder.Build();
    //
    //             // nacos 漏洞太多
    //             // // 1. 加载 nacos 配置
    //             // var section = configuration.GetSection("Nacos");
    //             // if (section.GetChildren().Any())
    //             // {
    //             //     builder.AddNacosV2Configuration(section);
    //             // }
    //
    //             // 2. 加载 remote configuration 配置
    //             if (!string.IsNullOrEmpty(configuration["RemoteConfiguration:Endpoint"]))
    //             {
    //                 builder.AddAliyunJsonFile(source =>
    //                 {
    //                     source.Endpoint = configuration["RemoteConfiguration:Endpoint"];
    //                     source.BucketName = configuration["RemoteConfiguration:BucketName"];
    //                     source.AccessKeyId = configuration["RemoteConfiguration:AccessKeyId"];
    //                     source.AccessKeySecret = configuration["RemoteConfiguration:AccessKeySecret"];
    //                     source.Key = configuration["RemoteConfiguration:Key"];
    //                 });
    //             }
    //
    //             builder.AddCommandLine(args);
    //
    //             var finalConfiguration = builder.Build();
    //             EnvironmentVariables.OrleansHostIP =
    //                 EnvironmentVariables.GetValue(finalConfiguration, "HOST_IP", "HostIP");
    //
    //             var serilogSection = finalConfiguration.GetSection("Serilog");
    //             if (serilogSection.GetChildren().Any())
    //             {
    //                 Log.Logger = new LoggerConfiguration().ReadFrom
    //                     .Configuration(finalConfiguration)
    //                     .CreateLogger();
    //             }
    //             else
    //             {
    //                 var logFile = Environment.GetEnvironmentVariable("LOG_PATH");
    //                 if (string.IsNullOrEmpty(logFile))
    //                 {
    //                     logFile = Environment.GetEnvironmentVariable("LOG");
    //                 }
    //
    //                 if (string.IsNullOrEmpty(logFile))
    //                 {
    //                     logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
    //                         "logs/log.txt".ToLowerInvariant());
    //                 }
    //
    //                 Log.Logger = new LoggerConfiguration()
    //                     .MinimumLevel.Information()
    //                     .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    //                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    //                     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    //                     .MinimumLevel.Override("System", LogEventLevel.Warning)
    //                     .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
    //                     .Enrich.FromLogContext()
    //                     // Serilog.Enrichers.Thread
    //                     .Enrich.WithThreadId()
    //                     // Serilog.Enrichers.Environment
    //                     .Enrich.WithMachineName()
    //                     .WriteTo.Console()
    //                     // .WriteTo.ClickHouse()
    //                     .WriteTo.Async(x => x.File(logFile, rollingInterval: RollingInterval.Day))
    //                     .CreateLogger();
    //             }
    //         })
    //         .ConfigureWebHostDefaults(webBuilder =>
    //         {
    //             webBuilder.UseUrls("http://+:8200");
    //             webBuilder.UseStartup<Startup>();
    //         }).ConfigureSilo()
    //         .UseSerilog();
}
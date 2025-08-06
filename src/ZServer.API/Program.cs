using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using Serilog.Events;
using ZMap;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.Silo;
using Log = Serilog.Log;

#if !DEBUG
#endif

namespace ZServer.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // var binaryReader = new BinaryReader(new FileStream("/Users/lewis/Downloads/BigTIFFLong8Tiles.tif",
        //     FileMode.Open, FileAccess.Read));
        // var tiff = new TIFF.NET.TIFF(binaryReader);
        //
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

        var app = CreateHostBuilder(args).Build();
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        await DownloadResource(configuration, "FontResource", "fonts");
        FontUtility.Load();
        await DownloadResource(configuration, "SymbolResource", "symbols");
        await app.RunAsync();
    }

    private static async Task DownloadResource(IConfiguration configuration, string name, string folder)
    {
        var url = configuration[name];
        var httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(url))
        {
            var bytes = await httpClient.GetByteArrayAsync(url);
            if (bytes.Length > 0)
            {
                var fontFile = Path.Combine("/tmp", $"{Guid.NewGuid():N}.zip");
                if (!Directory.Exists(Path.GetDirectoryName(fontFile)))
                {
                    Directory.CreateDirectory("/tmp");
                }

                await File.WriteAllBytesAsync(fontFile, bytes);
                // 解压文件
                using var zipArchive = ZipFile.OpenRead(fontFile);
                foreach (var entry in zipArchive.Entries)
                {
                    // 构建目标文件路径
                    var destinationPath = Path.Combine($"/app/{folder}", entry.FullName);
                    // 解压文件
                    entry.ExtractToFile(destinationPath, false);
                }
            }
        }
    }


    // private static void FixOrleansPublishSingleFileIssue()
    // {
    //     var assembly = typeof(Orleans.Runtime.SiloStatus).Assembly;
    //
    //     if (!string.IsNullOrWhiteSpace(assembly.Location))
    //     {
    //         return;
    //     }
    //
    //     var type = typeof(Orleans.Runtime.SiloStatus).Assembly.GetTypes()
    //         .First(
    //             x => x.FullName == "Orleans.Runtime.RuntimeVersion");
    //     var method = type.GetProperty("Current")?.GetMethod;
    //     if (method == null)
    //     {
    //         return;
    //     }
    //
    //     var harmony = new Harmony("orleans.publishSingleFile");
    //     var prefix = typeof(Program).GetMethod("Prefix");
    //     harmony.Patch(method, new HarmonyMethod(prefix));
    //     Console.WriteLine("Patch Orleans completed");
    // }

    // public static bool Prefix(ref string __result)
    // {
    //     __result = "3.6.5";
    //     return false; // make sure you only skip if really necessary
    // }

    /// <summary>
    /// 配置响应顺序，按从低到高：环境 -> 配置 -> command parameters
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(x =>
            {
                x.AddEnvironmentVariables();
                x.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddEnvironmentVariables();

                if (File.Exists("conf/serilog.json"))
                {
                    builder.AddJsonFile("conf/serilog.json", optional: true, reloadOnChange: true);
                }

                if (File.Exists("conf/appsettings.json"))
                {
                    builder.AddJsonFile("conf/appsettings.json", optional: true, reloadOnChange: true);
                }

                var configuration = builder.Build();

                // nacos 漏洞太多
                // // 1. 加载 nacos 配置
                // var section = configuration.GetSection("Nacos");
                // if (section.GetChildren().Any())
                // {
                //     builder.AddNacosV2Configuration(section);
                // }

                // 2. 加载 remote configuration 配置
                if (!string.IsNullOrEmpty(configuration["RemoteConfiguration:Endpoint"]))
                {
                    builder.AddAliyunJsonFile(source =>
                    {
                        source.Endpoint = configuration["RemoteConfiguration:Endpoint"];
                        source.BucketName = configuration["RemoteConfiguration:BucketName"];
                        source.AccessKeyId = configuration["RemoteConfiguration:AccessKeyId"];
                        source.AccessKeySecret = configuration["RemoteConfiguration:AccessKeySecret"];
                        source.Key = configuration["RemoteConfiguration:Key"];
                    });
                }

                builder.AddCommandLine(args);

                var finalConfiguration = builder.Build();
                EnvironmentVariables.OrleansHostIP =
                    EnvironmentVariables.GetValue(finalConfiguration, "HOST_IP", "HostIP");

                var serilogSection = finalConfiguration.GetSection("Serilog");
                if (serilogSection.GetChildren().Any())
                {
                    Log.Logger = new LoggerConfiguration().ReadFrom
                        .Configuration(finalConfiguration)
                        .CreateLogger();
                }
                else
                {
                    var logFile = Environment.GetEnvironmentVariable("LOG_PATH");
                    if (string.IsNullOrEmpty(logFile))
                    {
                        logFile = Environment.GetEnvironmentVariable("LOG");
                    }

                    if (string.IsNullOrEmpty(logFile))
                    {
                        logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                            "logs/log.txt".ToLowerInvariant());
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
                        .WriteTo.Async(x => x.File(logFile, rollingInterval: RollingInterval.Day))
                        .CreateLogger();
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://+:8200");
                webBuilder.UseStartup<Startup>();
            }).ConfigureSilo()
            .UseSerilog();
}
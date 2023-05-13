using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.API.Extensions;
using Log = Serilog.Log;

#if !DEBUG
#endif

namespace ZServer.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // FixOrleansPublishSingleFileIssue();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            NtsGeometryServices.Instance = new NtsGeometryServices(
                CoordinateArraySequenceFactory.Instance,
                PrecisionModel.Floating.Value,
                4490, GeometryOverlay.Legacy, new CoordinateEqualityComparer());
            FontUtilities.Load();
            CSharpDynamicCompiler.Load<NatashaDynamicCompiler>();
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            if (!Directory.Exists("cache"))
            {
                Directory.CreateDirectory("cache");
            }

            CreateHostBuilder(args).Build().Run();
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder.AddCommandLine(args);

                    if (File.Exists("serilog.json"))
                    {
                        builder.AddJsonFile($"serilog.json",
                            optional: true, reloadOnChange: true);
                    }

                    if (File.Exists("zserver.json"))
                    {
                        builder.AddJsonFile($"zserver.json",
                            optional: true, reloadOnChange: true);
                    }

                    var configuration = builder.Build();

                    // 1. 加载 nacos 配置
                    var section = configuration.GetSection("Nacos");
                    if (section.GetChildren().Any())
                    {
                        builder.AddNacosV2Configuration(section);
                    }

                    // 2. 加载 remote configuration 配置
                    if (!string.IsNullOrWhiteSpace(configuration["RemoteConfiguration:Endpoint"]))
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

                    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Build()).CreateLogger();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://+:8200");
                }).ConfigureOrleans()
                .UseSerilog();
    }
}
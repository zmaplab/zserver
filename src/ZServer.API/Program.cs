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
using ZMap;
using ZMap.DynamicCompiler;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source.Postgre;
using ZServer.API.Extensions;
using Log = Serilog.Log;

#if !DEBUG
#endif

namespace ZServer.API
{
    public static class Program
    {
        public static object R(Feature feature)
        {
          return  () => { var v = feature["dasharray"]?.ToString() as string; return v?.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Select(x => (System.Single)System.Convert.ChangeType(x, typeof(System.Single))).ToArray(); };
        }
        public static void Main(string[] args)
        {
            // FixOrleansPublishSingleFileIssue();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            NtsGeometryServices.Instance = new NtsGeometryServices(
                CoordinateArraySequenceFactory.Instance,
                PrecisionModel.Floating.Value,
                4490, GeometryOverlay.Legacy, new CoordinateEqualityComparer());
            PostgreSource.Initialize();
            FontUtilities.Load();
            CSharpDynamicCompiler.Load();
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

                    // 1. ?????? nacos ??????
                    var section = configuration.GetSection("Nacos");
                    if (section.GetChildren().Any())
                    {
                        builder.AddNacosV2Configuration(section);
                    }

                    // 2. ?????? remote configuration ??????
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
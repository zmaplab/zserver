using System;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using HarmonyLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Serilog;
using ZMap.DynamicCompiler;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source.Postgre;
using ZServer.API.Extensions;

#if !DEBUG
#endif

namespace ZServer.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            FixOrleansPublishSingleFileIssue();

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

        private static void FixOrleansPublishSingleFileIssue()
        {
            var assembly = typeof(Orleans.Runtime.SiloStatus).Assembly;

            // if (!string.IsNullOrWhiteSpace(assembly.Location))
            // {
            //     return;
            // }

            var type = typeof(Orleans.Runtime.SiloStatus).Assembly.GetTypes()
                .First(
                    x => x.FullName == "Orleans.Runtime.RuntimeVersion");
            var method = type.GetProperty("Current")?.GetMethod;
            if (method == null)
            {
                return;
            }

            var harmony = new Harmony("orleans.publishSingleFile");
            var prefix = typeof(Program).GetMethod("Prefix");
            harmony.Patch(method, new HarmonyMethod(prefix));
            Console.WriteLine("Patch Orleans completed");
        }

        public static bool Prefix(ref string __result)
        {
            __result = "3.6.5";
            return false; // make sure you only skip if really necessary
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder.AddCommandLine(args);
                    builder.AddJsonFile($"serilog.json",
                        optional: true, reloadOnChange: true);
                    builder.AddJsonFile($"zserver.json",
                        optional: true, reloadOnChange: true);

                    if (File.Exists("nacos.json"))
                    {
                        var configurationBuilder = new ConfigurationBuilder();
                        configurationBuilder.AddJsonFile("nacos.json");
                        var configuration = configurationBuilder.Build();
                        var section = configuration.GetSection("Nacos");
                        if (section.GetChildren().Any())
                        {
                            builder.AddNacosV2Configuration(section);
                        }
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
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Serilog;
using ZMap.DynamicCompiler;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZMap.Source.Postgre;
using ZMap.Utilities;
using ZServer.API.Extensions;

#if !DEBUG
#endif

namespace ZServer.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
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
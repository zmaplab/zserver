using System.IO;
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
using ZMap.Infrastructure;
// using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.Silo;
using Log = Serilog.Log;

#if !DEBUG
#endif

namespace ZServer.API
{
    public static class Program
    {
        private static IConfiguration _configuration;

        public static void Main(string[] args)
        {
            Utilities.PrintInfo();
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
                        builder.AddJsonFile($"conf/serilog.json",
                            optional: true, reloadOnChange: true);
                    }

                    if (File.Exists("conf/appsettings.json"))
                    {
                        builder.AddJsonFile($"conf/appsettings.json",
                            optional: true, reloadOnChange: true);
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

                    builder.AddCommandLine(args);

                    _configuration = builder.Build();

                    EnvironmentVariables.HostIP = EnvironmentVariables.GetValue(_configuration, "HOST_IP", "HostIP");

                    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configurationBuilder = new ConfigurationBuilder();
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddCommandLine(args);
                    var configuration = configurationBuilder.Build();

                    webBuilder.UseStartup<Startup>();

                    var port = configuration["PORT"];
                    if (string.IsNullOrEmpty(port))
                    {
                        port = "8200";
                    }

                    EnvironmentVariables.Port = port;

                    webBuilder.UseUrls($"http://+:{EnvironmentVariables.Port}");
                }).ConfigureSilo()
                .UseSerilog();
    }
}
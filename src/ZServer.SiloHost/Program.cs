using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using ZMap.DynamicCompiler;
using ZMap.Infrastructure;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.SiloHost.Extensions;
using Log = Serilog.Log;

namespace ZServer.SiloHost
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
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

            return new HostBuilder()
                .ConfigureHostConfiguration(x =>
                {
                    x.AddEnvironmentVariables();
                    x.AddCommandLine(args);
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                    builder.AddCommandLine(args);

                    if (File.Exists("conf/serilog.json"))
                    {
                        builder.AddJsonFile("conf/serilog.json",
                            optional: true, reloadOnChange: true);
                    }

                    if (File.Exists("conf/appsettings.json"))
                    {
                        builder.AddJsonFile("conf/appsettings.json",
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
                .UseOrleans(OrleansExtensions.ConfigureSilo)
                .UseSerilog()
                .RunConsoleAsync();
        }
    }
}
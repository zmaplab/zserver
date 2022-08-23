using System;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using Npgsql;
using ZMap.Source.Postgre;
using Orleans;
using Serilog;
using Serilog.Events;
using ZMap.DynamicCompiler;
using ZMap.Renderer.SkiaSharp.Utilities;
using ZServer.Grains.WMS;

namespace ZServer.SiloHost
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CSharpDynamicCompiler.Load();

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Debug()
#else
                    .MinimumLevel.Information()
#endif
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
#if DEBUG
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
#endif
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                    .MinimumLevel.Override("Orleans", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                ;
            if (configuration["seq_server_url"] != null)
            {
                var apiKey = string.IsNullOrWhiteSpace(configuration["seq_api_key"])
                    ? null
                    : configuration["seq_api_key"];
                loggerConfiguration.WriteTo.Seq(configuration["seq_server_url"], apiKey: apiKey);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            PostgreSource.Initialize();
            FontUtilities.Load();

            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            NtsGeometryServices.Instance = new NtsGeometryServices(
                CoordinateArraySequenceFactory.Instance,
                PrecisionModel.Floating.Value,
                4490, GeometryOverlay.Legacy, new CoordinateEqualityComparer());

            return new HostBuilder()
                .ConfigureHostConfiguration(x =>
                {
                    x.AddEnvironmentVariables();
                    x.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var environment = context.Configuration["DOTNET_ENVIRONMENT"];
                    builder.AddJsonFile($"appsettings.json");

                    builder.AddEnvironmentVariables();
                    builder.AddCommandLine(args);

                    var nacosConfig = "appsettings.Nacos.json";
                    if (environment != "Development" && File.Exists(nacosConfig))
                    {
                        Console.WriteLine(File.ReadAllText(nacosConfig));
                        builder.AddNacosV2Configuration(new ConfigurationBuilder()
                            .AddJsonFile(nacosConfig)
                            .Build());
                    }
                })
                .UseOrleans((context, builder) =>
                {
                    var connectString = context.Configuration["Orleans:ConnectionString"];
                    var invariant = context.Configuration["Orleans:Invariant"];

                    var assembly = Assembly.Load($"{invariant}");
                    if (assembly == null)
                    {
                        throw new Exception("Invariant driver is missing");
                    }

                    var scriptPath = $"{invariant}.sql";
                    if (!File.Exists(scriptPath))
                    {
                        throw new Exception("Invariant sql is missing");
                    }

                    var sql = File.ReadAllText(scriptPath);

                    var connectionType =
                        assembly.GetTypes().FirstOrDefault(x => x.IsAssignableTo(typeof(IDbConnection)));
                    if (connectionType == null)
                    {
                        throw new Exception("Invariant driver's connection type is missing");
                    }

                    Console.WriteLine("Orleans:ConnectionString" + connectString);
                    using var conn = (IDbConnection)Activator.CreateInstance(connectionType, connectString);
                    try
                    {
                        conn.Query($"SELECT * FROM orleansmembershiptable LIMIT 0");
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("\"orleansmembershiptable\" does not exist"))
                        {
                            conn.Execute(sql);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    builder
                        .AddMemoryGrainStorageAsDefault()
                        .UseAdoNetClustering(options =>
                        {
                            options.ConnectionString = context.Configuration["Orleans:ConnectionString"];
                            options.Invariant = context.Configuration["Orleans:Invariant"];
                        })
                        .UseAdoNetReminderService(options =>
                        {
                            options.ConnectionString = connectString;
                            options.Invariant = context.Configuration["Orleans:Invariant"];
                        })
                        .Configure<SiloOptions>(options => options.SiloName = context.Configuration["Orleans:SiloName"])
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = context.Configuration["Orleans:ClusterId"];
                            options.ServiceId = context.Configuration["Orleans:ServiceId"];
                        })
                        .Configure<EndpointOptions>(options =>
                        {
                            var siloPort = int.Parse(context.Configuration["Orleans:SiloPort"]);
                            var gatewayPort = int.Parse(context.Configuration["Orleans:GatewayPort"]);
                            options.GatewayPort = gatewayPort;
                            options.SiloPort = siloPort;

                            var hostIp = Environment.GetEnvironmentVariable("HOST_IP");
                            IPAddress ipAddress;
                            if (string.IsNullOrWhiteSpace(hostIp))
                            {
                                var ips = Dns.GetHostAddressesAsync(Dns.GetHostName()).Result;
                                ipAddress = ips.FirstOrDefault(x =>
                                    x.AddressFamily == AddressFamily.InterNetwork
                                    && !Equals(x, IPAddress.Loopback)
                                    && !Equals(x, IPAddress.Any)
                                    && !Equals(x, IPAddress.Broadcast)
                                    && !Equals(x, IPAddress.None));
                            }
                            else
                            {
                                ipAddress = IPAddress.Parse(hostIp);
                            }

                            options.AdvertisedIPAddress = ipAddress;
                            options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, siloPort);
                            options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, gatewayPort);
                        })
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(WMSGrain).Assembly).WithReferences());
                    if (context.Configuration["Orleans:Dashboard"] == "True")
                    {
                        builder.UseDashboard(options => { options.Port = 8082; });
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddZServer(context.Configuration);
                    services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });
                })
                .ConfigureLogging(builder =>
                {
                    var builder2 = builder.AddSerilog();
                    ZMap.Log.Logger =
                        builder2.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Default");
                })
                .RunConsoleAsync();
        }
    }
}
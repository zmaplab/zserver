using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using ZMap;
using ZServer.Grains.WMS;

namespace ZServer.API.Extensions;

public static class OrleansExtensions
{
    public static IHostBuilder ConfigureOrleans(this IHostBuilder builder)
    {
        builder.UseOrleans((context, siloBuilder) =>
        {
            siloBuilder.AddMemoryGrainStorageAsDefault();

            if ("true".Equals(context.Configuration["standalone"]) ||
                "true".Equals(Environment.GetEnvironmentVariable("standalone")?.ToLower()))
            {
                siloBuilder.UseLocalhostClustering(11111, 30000, null, "zserver", "zserver");
                siloBuilder.UseInMemoryReminderService();
                Log.Logger.Information("Standalone: true");
            }
            else
            {
                var connectString = context.Configuration["Orleans:ConnectionString"];
                var invariant = context.Configuration["Orleans:Invariant"];

                var assembly = Assembly.Load($"{invariant}");

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

                using var conn =
                    (IDbConnection)Activator.CreateInstance(connectionType, connectString);
                try
                {
                    conn.Query($"SELECT * FROM orleansmembershiptable LIMIT 0");
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("\"orleansmembershiptable\" does not exist") || e.Message.Contains("不存在"))
                    {
                        conn.Execute(sql);
                    }
                    else
                    {
                        throw;
                    }
                }

                siloBuilder.UseAdoNetClustering(options =>
                {
                    options.ConnectionString = context.Configuration["Orleans:ConnectionString"];
                    options.Invariant = context.Configuration["Orleans:Invariant"];
                });
                siloBuilder.UseAdoNetReminderService(options =>
                {
                    options.ConnectionString = connectString;
                    options.Invariant = context.Configuration["Orleans:Invariant"];
                });
                siloBuilder.Configure<SiloOptions>(options =>
                        options.SiloName = context.Configuration["Orleans:SiloName"])
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

                        var hostIp = EnvironmentVariables.HostIp;
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
                    });
            }

            siloBuilder.ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(WMSGrain).Assembly).WithReferences());
            if (context.Configuration["Orleans:Dashboard"] == "True")
            {
                siloBuilder.UseDashboard(options => { options.Port = 8182; });
            }
        });
        return builder;
    }
}
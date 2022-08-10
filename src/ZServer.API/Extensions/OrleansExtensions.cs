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
using ZServer.Grains.WMS;

namespace ZServer.API.Extensions;

public static class OrleansExtensions
    {
        public static IHostBuilder ConfigureOrleans(this IHostBuilder builder)
        {
            builder.UseOrleans((context, siloBuilder) =>
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
                    if (e.Message.Contains("\"orleansmembershiptable\" does not exist"))
                    {
                        conn.Execute(sql);
                    }
                    else
                    {
                        throw;
                    }
                }

                siloBuilder
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
                    siloBuilder.UseDashboard(options => { options.Port = 8182; });
                }
            });
            return builder;
        }
    }
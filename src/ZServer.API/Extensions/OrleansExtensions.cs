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

namespace ZServer.API.Extensions;

public static class OrleansExtensions
{
    public static void ConfigureSilo(HostBuilderContext context, ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();

        if ("true".Equals(context.Configuration["standalone"]) ||
            "true".Equals(Environment.GetEnvironmentVariable("standalone")?.ToLower()))
        {
            siloBuilder.UseLocalhostClustering(11111, 30000, null, "zserver", "zserver");
            siloBuilder.UseInMemoryReminderService();
            Log.Logger.Information("Standalone: true");
            return;
        }

        var connectString = EnvironmentVariables.GetValue(context.Configuration, "ClusterConnectionString",
            "Orleans:ConnectionString");
        var invariant = EnvironmentVariables.GetValue(context.Configuration, "ClusterInvariant", "Orleans:Invariant");
        var siloName = EnvironmentVariables.GetValue(context.Configuration, "ClusterSiloName", "Orleans:SiloName");
        var clusterId = EnvironmentVariables.GetValue(context.Configuration, "ClusterId", "Orleans:ClusterId");
        var serviceId = EnvironmentVariables.GetValue(context.Configuration, "ClusterServiceId", "Orleans:ServiceId");
        var siloPort =
            int.Parse(EnvironmentVariables.GetValue(context.Configuration, "ClusterSiloPort", "Orleans:SiloPort"));
        var gatewayPort =
            int.Parse(EnvironmentVariables.GetValue(context.Configuration, "ClusterGatewayPort",
                "Orleans:GatewayPort"));
        var enableDashboard =
            EnvironmentVariables.GetValue(context.Configuration, "ClusterDashboard", "Orleans:Dashboard");
        var dashboardPort =
            EnvironmentVariables.GetValue(context.Configuration, "ClusterDashboardPort", "Orleans:DashboardPort");
        if (string.IsNullOrEmpty(dashboardPort))
        {
            dashboardPort = "8182";
        }

        Log.Logger.Information(
            $"Standalone: false, Invariant: {invariant}, SiloName: {siloName}, ClusterId: {clusterId}, ServiceId: {serviceId}, SiloPort: {siloPort}, GatewayPort: {gatewayPort}, API: {EnvironmentVariables.Port}, Dashboard: {enableDashboard}, DashboardPort: {dashboardPort}");
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
        conn.Execute(sql);

        siloBuilder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = connectString;
            options.Invariant = invariant;
        });
        siloBuilder.UseAdoNetReminderService(options =>
        {
            options.ConnectionString = connectString;
            options.Invariant = invariant;
        });
        siloBuilder.Configure<SiloOptions>(options => options.SiloName = siloName)
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = clusterId;
                options.ServiceId = serviceId;
            })
            .Configure<EndpointOptions>(options =>
            {
                options.SiloPort = siloPort;
                options.GatewayPort = gatewayPort;

                var hostIp = EnvironmentVariables.HostIP;
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
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, options.SiloPort);
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, options.GatewayPort);
            });

        if ("true".Equals(enableDashboard, StringComparison.OrdinalIgnoreCase))
        {
            siloBuilder.UseDashboard(options => { options.Port = int.Parse(dashboardPort); });
        }
    }
}
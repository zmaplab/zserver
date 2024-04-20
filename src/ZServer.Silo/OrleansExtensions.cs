using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using ZMap;
using ZMap.Infrastructure;

namespace ZServer.Silo;

public static class OrleansExtensions
{
    public static IHostBuilder ConfigureSilo(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseOrleans(ConfigureSilo);
        return hostBuilder;
    }

    private static void ConfigureSilo(HostBuilderContext context, ISiloBuilder siloBuilder)
    {
        var enableDashboard =
            EnvironmentVariables.GetValue(context.Configuration, "ClusterDashboard", "Orleans:Dashboard");
        var dashboardPort =
            EnvironmentVariables.GetValue(context.Configuration, "ClusterDashboardPort", "Orleans:DashboardPort");
        if (string.IsNullOrEmpty(dashboardPort))
        {
            dashboardPort = "8182";
        }

        siloBuilder.AddMemoryGrainStorageAsDefault();

        if ("true".Equals(context.Configuration["standalone"], StringComparison.OrdinalIgnoreCase))
        {
            siloBuilder.UseLocalhostClustering(11111, 30000, null, "zserver", "zserver");
            siloBuilder.UseInMemoryReminderService();
            if ("true".Equals(enableDashboard, StringComparison.OrdinalIgnoreCase))
            {
                siloBuilder.UseDashboard(options => { options.Port = int.Parse(dashboardPort); });
            }

            Log.Logger.LogInformation(
                $"Standalone: true, API: {EnvironmentVariables.Port}, Dashboard: {enableDashboard}, DashboardPort: {dashboardPort}");
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

        Log.Logger.LogInformation(
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
            Activator.CreateInstance(connectionType, connectString) as IDbConnection;
        if (conn == null)
        {
            throw new ArgumentException("无法创建数据库连接");
        }

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
        // comments by lewis at 20230923
        // 如 WMTS 的 tile, 不需要保持状态
        // siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
        // {
        //     options.ConnectionString = connectString;
        //     options.Invariant = invariant;
        // });
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
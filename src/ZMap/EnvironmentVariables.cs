using System;
using Microsoft.Extensions.Configuration;

namespace ZMap;

public static class EnvironmentVariables
{
    public static readonly bool EnableSensitiveDataLogging;
    public static string HostIP;
    public static string Port = "8200";

    static EnvironmentVariables()
    {
        EnableSensitiveDataLogging = string.Equals("true",
            Environment.GetEnvironmentVariable("EnableSensitiveDataLogging"),
            StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetValue(IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var v = configuration[key];
            if (!string.IsNullOrEmpty(v))
            {
                return v;
            }
        }

        return null;
    }
}
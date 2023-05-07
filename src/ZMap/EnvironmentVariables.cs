using System;

namespace ZMap;

public static class EnvironmentVariables
{
    public static readonly bool EnableSensitiveDataLogging;
    public static readonly string HostIp;

    static EnvironmentVariables()
    {
        EnableSensitiveDataLogging = string.Equals("true",
            Environment.GetEnvironmentVariable("EnableSensitiveDataLogging"),
            StringComparison.InvariantCultureIgnoreCase);
        HostIp = Environment.GetEnvironmentVariable("HOST_IP");
    }
}
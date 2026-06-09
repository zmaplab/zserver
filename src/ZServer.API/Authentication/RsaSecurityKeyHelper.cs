using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ZServer.API.Authentication;

/// <summary>
/// 
/// </summary>
public static class RsaSecurityKeyHelper
{
    private static readonly ConcurrentDictionary<string, RsaSecurityKey> Cache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyPath"></param>
    /// <returns></returns>
    public static RsaSecurityKey GetRsaSecurityKey(string keyPath)
    {
        if (string.IsNullOrEmpty(keyPath))
        {
            return null;
        }

        return Cache.GetOrAdd(keyPath, path =>
        {
            var logger = ZMap.Infrastructure.Log.CreateLogger("RsaSecurityKeyHelper");
            try
            {
                var json = System.IO.File.ReadAllText(keyPath);
                var document = JsonSerializer.Deserialize<JsonDocument>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                if (document == null)
                {
                    return null;
                }

                var n = document.RootElement.GetString("n");
                var e = document.RootElement.GetString("e");
                var d = document.RootElement.GetString("d");
                var p = document.RootElement.GetString("p");
                var q = document.RootElement.GetString("q");
                var dp = document.RootElement.GetString("dp");
                var dq = document.RootElement.GetString("dq");
                var inverseQ = document.RootElement.GetString("qi");

                var parameters = new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(n),
                    Exponent = Base64UrlEncoder.DecodeBytes(e),
                    D = string.IsNullOrEmpty(d)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(d),
                    P = string.IsNullOrEmpty(p)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(p),
                    Q = string.IsNullOrEmpty(q)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(q),
                    DP = string.IsNullOrEmpty(dp)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(dp),
                    DQ = string.IsNullOrEmpty(dq)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(dq),
                    InverseQ = string.IsNullOrEmpty(inverseQ)
                        ? null
                        : Base64UrlEncoder.DecodeBytes(inverseQ)
                };
                return new RsaSecurityKey(parameters);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error loading RSA key from {path}");
                return null;
            }
        });
    }
}
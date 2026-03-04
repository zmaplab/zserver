using System;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace ZServer.API;

/// <summary>
/// 
/// </summary>
public class JwtBearerSettings
{
    /// <summary>
    /// 
    /// </summary>
    public static string JwtBearerRsaSecurityKey = "JwtBearerRsaSecurityKey";

    /// <summary>
    /// 
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string ValidIssuer { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ValidAudience { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public RSAParametersInfo Key { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string MetadataAddress { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string KeyPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetMetadataAddress()
    {
        // 试验性代码，authority 不设计 https/requireHttpsMetadata
        if (!RequireHttpsMetadata && string.IsNullOrEmpty(MetadataAddress) &&
            !string.IsNullOrEmpty(Authority))
        {
            var metadataAddress =
                Authority.Replace("https://", "http://", StringComparison.OrdinalIgnoreCase);
            if (!metadataAddress.EndsWith("/", StringComparison.Ordinal))
            {
                metadataAddress += "/";
            }

            return metadataAddress + ".well-known/openid-configuration";
        }

        return MetadataAddress ?? string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RsaSecurityKey GetRsaSecurityKey()
    {
        if (string.IsNullOrEmpty(KeyPath))
        {
            return Key?.GetRsaSecurityKey();
        }

        var json = System.IO.File.ReadAllText(KeyPath);
        Key = JsonSerializer.Deserialize<RSAParametersInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Key?.GetRsaSecurityKey();
    }
}
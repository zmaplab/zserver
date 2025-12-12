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
}
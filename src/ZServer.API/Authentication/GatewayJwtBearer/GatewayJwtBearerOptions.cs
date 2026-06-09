using Microsoft.AspNetCore.Authentication;

namespace ZServer.API.Authentication.GatewayJwtBearer;

/// <summary>
///
/// </summary>
public class GatewayJwtBearerOptions
    : AuthenticationSchemeOptions
{
    /// <summary>
    ///
    /// </summary>
    public string AuthenticationType { get; set; } = "GatewayBearer";

    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; } = "X-Userinfo";

    /// <summary>
    /// 若设置了则表示需要验证
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// 若设置了则表示需要验证
    /// </summary>
    public string Audience { get; set; }
}
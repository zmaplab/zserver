using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace ZServer.API.Authentication;

/// <summary>
/// 
/// </summary>
public class TokenAuthOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string AuthenticationType { get; set; } = "Token";

    /// <summary>
    /// 
    /// </summary>
    public HashSet<string> Tokens { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public string ApiName { get; set; }
}
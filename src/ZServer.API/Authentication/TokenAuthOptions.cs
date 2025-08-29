using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace ZServer.API.Authentication;

public class TokenAuthOptions : AuthenticationSchemeOptions
{
    public string AuthenticationType { get; set; } = "Token";
    public HashSet<string> Tokens { get; set; } = new();
    public string ApiName { get; set; }
}
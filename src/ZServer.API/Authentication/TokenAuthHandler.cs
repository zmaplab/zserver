using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZServer.API.Authentication;

/// <summary>
/// 
/// </summary>
/// <param name="options"></param>
/// <param name="logger"></param>
/// <param name="encoder"></param>
public class TokenAuthHandler(IOptionsMonitor<TokenAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<TokenAuthOptions>(options, logger, encoder)
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.User.Identity is { IsAuthenticated: true })
        {
            return AuthenticateResult.NoResult();
        }

        var actionContext = Context.GetEndpoint();
        if (actionContext == null)
        {
            return AuthenticateResult.Fail("No endpoint found");
        }

        var token = await GetValueAsync("z-tk");
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        if (token.Length != 24)
        {
            Logger.LogError("traceId {TraceId}: token 长度必须小于等于 24", Context.TraceIdentifier);
            return AuthenticateResult.Fail("401");
        }

        if (!Options.Tokens.Contains(token))
        {
            Logger.LogError("traceId {TraceId}: token 不合法", Context.TraceIdentifier);
            return AuthenticateResult.Fail("401");
        }

        Logger.LogDebug("验签成功 traceId {TraceId}", Context.TraceIdentifier);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, token),
            new(ClaimTypes.NameIdentifier, token),
            new(ClaimTypes.AuthenticationMethod, "Token"),
            new("scope", Options.ApiName),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    private Task<string> GetValueAsync(string key)
    {
        var v = Context.Request.Query[key].ToString();
        if (string.IsNullOrEmpty(v))
        {
            v = Context.Request.Headers[key].ToString();
        }

        return Task.FromResult(v);
    }
}
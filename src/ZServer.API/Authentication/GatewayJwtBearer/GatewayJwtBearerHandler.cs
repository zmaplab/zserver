using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZServer.API.Authentication.GatewayJwtBearer;

/// <summary>
/// TODO: 优化 claims 查询性能
/// </summary>
public class GatewayJwtBearerHandler : AuthenticationHandler<GatewayJwtBearerOptions>
{
    private readonly JsonOptions _jsonOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="encoder"></param>
    /// <param name="clock"></param>
    /// <param name="jsonOptions"></param>
    public GatewayJwtBearerHandler(IOptionsMonitor<GatewayJwtBearerOptions> options, ILoggerFactory logger,
#pragma warning disable CS0618 // Type or member is obsolete
        UrlEncoder encoder, ISystemClock clock, IOptions<JsonOptions> jsonOptions) : base(options, logger, encoder,
        clock)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        _jsonOptions = jsonOptions.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="encoder"></param>
    /// <param name="jsonOptions"></param>
    public GatewayJwtBearerHandler(IOptionsMonitor<GatewayJwtBearerOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, IOptions<JsonOptions> jsonOptions) : base(options, logger, encoder)
    {
        _jsonOptions = jsonOptions.Value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;
        var options = OptionsMonitor.CurrentValue;
        var headerName = options.Name;
        if (!Context.Request.Headers.ContainsKey(headerName))
        {
            return AuthenticateResult.NoResult();
        }

        var base64 = Context.Request.Headers[headerName].ToString();
        if (string.IsNullOrEmpty(base64))
        {
            return AuthenticateResult.NoResult();
        }

        AuthenticateResult result;
        try
        {
            var json = Convert.FromBase64String(base64);

            using var memoryStream = new MemoryStream(json);
            var profile =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(memoryStream,
                    _jsonOptions.JsonSerializerOptions);
            if (profile == null)
            {
                Logger.LogInformation("Deserialize X-Userinfo value failed");
                return AuthenticateResult.NoResult();
            }

            Logger.LogDebug("Deserialize X-Userinfo value success: {Profile}",
                JsonSerializer.Serialize(profile, _jsonOptions.JsonSerializerOptions));

            var claims = new List<Claim>();
            Add(claims, profile, "sub", ClaimTypes.NameIdentifier);
            Add(claims, profile, ClaimTypes.NameIdentifier, ClaimTypes.NameIdentifier);
            Add(claims, profile, "role", ClaimTypes.Role);
            Add(claims, profile, ClaimTypes.Role, ClaimTypes.Role);
            Add(claims, profile, "name", ClaimTypes.Name);
            Add(claims, profile, ClaimTypes.Name, ClaimTypes.Name);
            Add(claims, profile, "iss");
            Add(claims, profile, "aud");
            Add(claims, profile, "jti");
            Add(claims, profile, "exp");
            Add(claims, profile, "client_id");
            Add(claims, profile, "security-stamp");
            Add(claims, profile, "iat");
            Add(claims, profile, "sid");

            var jsonElement = profile["scope"];
            if (jsonElement != null)
            {
                var scope = jsonElement.Value.ToString();
                foreach (var s in scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    claims.Add(new Claim("scope", s));
                }
            }

            if (!string.IsNullOrEmpty(options.Issuer))
            {
                var iss = claims.FirstOrDefault(x => x.Type == "iss")?.Value;
                if (!options.Issuer.Equals(iss))
                {
                    return AuthenticateResult.Fail("Issuer is invalid");
                }
            }

            if (!string.IsNullOrEmpty(options.Audience))
            {
                if (!claims.Any(x => x.Type == "aud" && x.Value == options.Audience))
                {
                    return AuthenticateResult.Fail("Audience is invalid");
                }
            }

            var now = DateTimeOffset.UtcNow;
            var nbf = claims.FirstOrDefault(x => x.Type == "nbf")?.Value;
            if (nbf != null)
            {
                var notBefore = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nbf));
                if (now < notBefore)
                {
                    return AuthenticateResult.Fail("Token is not available");
                }
            }

            var exp = claims.FirstOrDefault(x => x.Type == "exp")?.Value;
            if (exp != null)
            {
                var expired = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
                if (now > expired)
                {
                    return AuthenticateResult.Fail("Token is expired");
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            result = AuthenticateResult.Success(ticket);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Handle X-Userinfo value failed");
            result = AuthenticateResult.Fail("Handle X-Userinfo value failed: " + e.Message);
        }

        return result;
    }

    private void Add(List<Claim> claims, Dictionary<string, JsonElement?> json, string key, string name = null)
    {
        if (!json.TryGetValue(key, out var jsonElement))
        {
            return;
        }

        if (jsonElement == null)
        {
            return;
        }

        var property = name ?? key;
        if (jsonElement.Value.ValueKind == JsonValueKind.String)
        {
            var v = jsonElement.Value.GetString();
            if (!string.IsNullOrEmpty(v))
            {
                claims.Add(new Claim(property, v));
            }
        }
        else if (jsonElement.Value.ValueKind == JsonValueKind.Number)
        {
            claims.Add(new Claim(property, jsonElement.Value.GetInt64().ToString()));
        }
        else if (jsonElement.Value.ValueKind == JsonValueKind.True ||
                 jsonElement.Value.ValueKind == JsonValueKind.False)
        {
            claims.Add(new Claim(property, jsonElement.Value.GetBoolean().ToString()));
        }
        else if (jsonElement.Value.ValueKind == JsonValueKind.Array)
        {
            var v = jsonElement.Value.Deserialize<List<string>>();
            if (v != null)
            {
                foreach (var p in v)
                {
                    claims.Add(new Claim(property, p));
                }
            }
        }
        else
        {
            claims.Add(new Claim(property, jsonElement.Value.ToString()));
        }
    }
}
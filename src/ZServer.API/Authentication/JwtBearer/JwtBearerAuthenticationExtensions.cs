using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ZServer.API.Authentication.JwtBearer;

/// <summary>
/// 
/// </summary>
public static class JwtBearerAuthenticationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="apiName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services,
        AuthenticationBuilder builder,
        IConfiguration configuration, string apiName)
    {
        var jwtBearerOptions = configuration.GetSection("JwtBearer").Get<JwtBearerSettings>();
        if (jwtBearerOptions == null)
        {
            throw new ArgumentException("JwtBearer options not found in the configuration file.");
        }

        var rsaSecurityKey = RsaSecurityKeyHelper.GetRsaSecurityKey(jwtBearerOptions.KeyPath);
        if (rsaSecurityKey != null)
        {
            services.AddKeyedSingleton("JwtBearerRsaSecurityKey", rsaSecurityKey);
        }

        builder.AddJwtBearer("JwtBearer", options =>
        {
            // 1. 不要设置 Authority！
            options.Authority = null;

            // 2. 手动设置元数据地址（或直接给密钥）
            if (rsaSecurityKey != null)
            {
                // 可选：禁用自动发现配置的额外校验
                options.ConfigurationManager = null;
                options.TokenValidationParameters.IssuerSigningKey = rsaSecurityKey;
            }
            else
            {
                // 手动设置元数据地址
                options.MetadataAddress = jwtBearerOptions.GetMetadataAddress();
            }

            options.Audience = apiName;
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Aud 验证
                ValidateAudience = jwtBearerOptions.ValidateAudience,
                // 开启 Issuer 验证
                ValidateIssuer = jwtBearerOptions.ValidateIssuer,
                ValidIssuer = jwtBearerOptions.ValidIssuer,
                // 验证过期
                ValidateLifetime = jwtBearerOptions.ValidateLifetime
            };

            var metadataAddress = jwtBearerOptions.GetMetadataAddress();
            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever(new HttpClient(new HttpClientProxy()))
            );
            // 关键2：Token解析完成后，拦截拆分scope为多条Claim
            options.Events.OnTokenValidated = ctx =>
            {
                if (ctx.Principal == null)
                {
                    return Task.CompletedTask;
                }

                var scopeClaim = ctx.Principal.FindFirst("scope");
                if (scopeClaim != null && !string.IsNullOrWhiteSpace(scopeClaim.Value))
                {
                    var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (ctx.Principal.Identity is not ClaimsIdentity identity)
                    {
                        return Task.CompletedTask;
                    }

                    // 删除原始单条scope，插入多条独立scope claim
                    identity.RemoveClaim(scopeClaim);
                    foreach (var s in scopes)
                    {
                        identity.AddClaim(new Claim("scope", s));
                    }
                }

                return Task.CompletedTask;
            };
        });

        return builder;
    }

    private class JwtBearerSettings
    {
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool ValidateIssuer { get; set; } = true;
        public string ValidIssuer { get; set; }
        public bool ValidateLifetime { get; set; } = true;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string MetadataAddress { get; set; }
        public string KeyPath { get; set; }

        /// <summary>
        /// Authority: https + RequireHttpsMetadata: true ->  MetadataAddress: ""
        /// Authority: https + RequireHttpsMetadata: false ->  MetadataAddress: "http://"
        /// Authority: http + RequireHttpsMetadata: true ->  MetadataAddress: ""
        /// Authority: http + RequireHttpsMetadata: true ->  MetadataAddress: "http://"
        /// </summary>
        /// <returns></returns>
        public string GetMetadataAddress()
        {
            if (string.IsNullOrEmpty(MetadataAddress) &&
                !string.IsNullOrEmpty(Authority))
            {
                var authority = Authority.Replace("http://", string.Empty).Replace("https://", string.Empty)
                    .TrimEnd("/");
                var schema = RequireHttpsMetadata ? "https" : "http";
                return $"{schema}://{authority}/.well-known/openid-configuration";
            }

            throw new ArgumentException("Authority or MetadataAddress cannot be null or empty.");
        }
    }

    class HttpClientProxy : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            throw new HttpRequestException(
                $"Request: {request.RequestUri}, status code: {response.StatusCode}, response: {await response.Content.ReadAsStringAsync(cancellationToken)}");
        }
    }
}
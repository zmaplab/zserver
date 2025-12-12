using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ZServer.API;

/// <summary>
/// 
/// </summary>
public static class JwtBearerAuthenticationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtBearerOptions = configuration.GetSection("JwtBearer").Get<JwtBearerSettings>();
        RsaSecurityKey rsaSecurityKey = null;
        if (jwtBearerOptions.Key != null)
        {
            rsaSecurityKey = jwtBearerOptions.Key.GetRsaSecurityKey();
            services.AddKeyedSingleton(JwtBearerSettings.JwtBearerRsaSecurityKey, rsaSecurityKey);
        }

        var builder = services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
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
                    ValidateAudience = jwtBearerOptions.ValidateAudience,
                    ValidateIssuer = jwtBearerOptions.ValidateIssuer,
                    ValidIssuer = jwtBearerOptions.ValidIssuer,
                    ValidAudience = jwtBearerOptions.ValidAudience,
                    ValidateLifetime = jwtBearerOptions.ValidateLifetime
                };

                if (rsaSecurityKey != null)
                {
                    // 可选：禁用自动发现配置的额外校验
                    options.ConfigurationManager = null;
                    options.Authority = null;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.IssuerSigningKey = rsaSecurityKey;
                }
                else
                {
                    options.Authority = jwtBearerOptions.Authority ??
                                        throw new ApplicationException("JwtBearer:Authority is null or empty. ");

                    options.RequireHttpsMetadata = jwtBearerOptions.RequireHttpsMetadata;
                    options.MetadataAddress = jwtBearerOptions.MetadataAddress ?? string.Empty;

                    // 试验性代码，authority 不设计 https/requireHttpsMetadata
                    if (!options.RequireHttpsMetadata && string.IsNullOrEmpty(options.MetadataAddress) &&
                        !string.IsNullOrEmpty(options.Authority))
                    {
                        var metadataAddress =
                            options.Authority.Replace("https://", "http://", StringComparison.OrdinalIgnoreCase);
                        if (!metadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            metadataAddress += "/";
                        }

                        options.MetadataAddress = metadataAddress + ".well-known/openid-configuration";
                    }
                }
            });

        return builder;
    }
}
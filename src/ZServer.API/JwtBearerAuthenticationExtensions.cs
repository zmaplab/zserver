using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ZServer.API.Authentication;

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
    /// <param name="apiName"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static AuthenticationBuilder AddAuthentication(this IServiceCollection services,
        IConfiguration configuration, string apiName)
    {
        var jwtBearerOptions = configuration.GetSection("JwtBearer").Get<JwtBearerSettings>();
        var rsaSecurityKey = jwtBearerOptions.GetRsaSecurityKey();
        if (rsaSecurityKey != null)
        {
            services.AddKeyedSingleton(JwtBearerSettings.JwtBearerRsaSecurityKey, rsaSecurityKey);
        }

        var builder = services.AddAuthentication();
        builder.AddJwtBearer(options =>
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
                    options.Authority = jwtBearerOptions.Authority ?? throw new ApplicationException(
                        "JwtBearer:Authority is null or empty. Please check your configuration.");
                    options.RequireHttpsMetadata = jwtBearerOptions.RequireHttpsMetadata;
                    options.MetadataAddress = jwtBearerOptions.GetMetadataAddress();
                }
            });

        var tokens = configuration.GetSection("tokens").Get<HashSet<string>>();
        builder.AddScheme<TokenAuthOptions, TokenAuthHandler>("Token",
            opts =>
            {
                opts.Tokens = tokens;
                opts.ApiName = apiName;
            });

        return builder;
    }
}
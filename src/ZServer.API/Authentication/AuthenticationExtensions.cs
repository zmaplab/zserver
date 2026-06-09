using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMap.Infrastructure;
using ZServer.API.Authentication.GatewayJwtBearer;
using ZServer.API.Authentication.JwtBearer;
using ZServer.API.Authentication.Token;

namespace ZServer.API.Authentication;

/// <summary>
/// 
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="apiName"></param>
    /// <exception cref="ApplicationException"></exception>
    public static void AddApiAuthentication(this WebApplicationBuilder builder,
        string apiName)
    {
        var authenticationSchemeValue = builder.Configuration["AuthenticationSchemes"] ??
                                        "";
        var authenticationSchemes = authenticationSchemeValue.Split(',',
            StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

        var enableAuthorization = builder.Configuration.GetValue<bool>("EnableAuthorization");
        if (!enableAuthorization)
        {
            builder.Services.AddAuthentication();
            // 当授权被禁用时，添加一个“空”的授权服务或什么都不做。
            // 为了确保 MVC 能正常运行，我们可以添加一个允许所有请求的授权策略到全局过滤器。
            // 但更优雅的方式是通过自定义过滤器处理（见下一步）。
            // services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
            // 注册授权策略
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("default", policy => { policy.RequireAssertion(_ => true); });
            });

            return;
        }

        if (!authenticationSchemes.Any())
        {
            throw new ApplicationException(
                "EnableAuthorization is true, but AuthenticationSchemes are not configured in configuration.");
        }

        var logger = Log.CreateLogger("ZServer.API.Authentication");

        // 验证
        var authenticationBuilder = builder.Services.AddAuthentication();
        // JsonHeader Authentication
        if (authenticationSchemes.Contains("GatewayBearer", StringComparer.OrdinalIgnoreCase))
        {
            logger.LogInformation("Adding GatewayJwtBearer authentication");
            builder.Services.Configure<GatewayJwtBearerOptions>(builder.Configuration.GetSection("GatewayBearer"));
            authenticationBuilder
                .AddScheme<GatewayJwtBearerOptions, GatewayJwtBearerHandler>("GatewayBearer",
                    o =>
                    {
                        // 
                        o.Audience = apiName;
                    });
        }

        // JwtBearer Authentication
        if (authenticationSchemes.Contains("JwtBearer", StringComparer.OrdinalIgnoreCase))
        {
            logger.LogInformation("Adding JwtBearer authentication");
            builder.Services.AddJwtBearerAuthentication(authenticationBuilder, builder.Configuration, apiName);
        }

        if (authenticationSchemes.Contains("Token", StringComparer.OrdinalIgnoreCase))
        {
            var tokens = builder.Configuration.GetSection("tokens").Get<HashSet<string>>();
            if (!tokens.Any())
            {
                throw new ApplicationException("Tokens are not configured.");
            }

            authenticationBuilder.AddScheme<TokenAuthOptions, TokenAuthHandler>("Token",
                opts =>
                {
                    opts.Tokens = tokens;
                    opts.ApiName = apiName;
                });
        }

        // 注册授权策略
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("default", policy =>
            {
                policy.AddAuthenticationSchemes(authenticationSchemes);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", apiName);
            });
        });
    }
}
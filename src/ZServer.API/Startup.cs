using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.IO.Converters;
using Orleans.Configuration;
using Serilog.Context;
using ZMap.Permission;
using ZServer.API.Authentication;
using ZServer.API.Features;
using ZServer.API.Filters;
using ZServer.API.Permission;
using ZServer.API.Serilog;
using Log = ZMap.Infrastructure.Log;

namespace ZServer.API;

/// <summary>
/// 
/// </summary>
/// <param name="configuration"></param>
public class Startup(IConfiguration configuration)
{
    private bool _enableAuthorization;

    // This method gets called by the runtime. Use this method to add services to the container.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="ApplicationException"></exception>
    public void ConfigureServices(IServiceCollection services)
    {
        _enableAuthorization = configuration.GetValue<bool>("EnableAuthorization");

        services.AddOpenApi("zserver-api", options =>
        {
            // Specify the OpenAPI version to use
            options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        });
        // 替换默认的 IHttpRequestIdentifierFeature
        services.AddTransient<IHttpRequestIdentifierFeature>(sp =>
        {
            // 获取原始 Feature（框架默认实现）
            var originalFeature = sp.GetRequiredService<HttpRequestIdentifierFeature>();
            // 注入依赖项
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            // 返回自定义 Feature
            return new TraceIdentifierFeature(originalFeature, httpContextAccessor);
        });

        services.AddControllers(x => { x.Filters.Add<GlobalExceptionFilter>(); })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
            });

        services.AddResponseCaching();
        services.AddRouting(x => { x.LowercaseUrls = true; });
        services.AddZServer(configuration).AddSkiaSharpRenderer();
        services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });
        services.Configure<ServerOptions>(configuration);
        services.Configure<ClusterOptions>("Orleans", configuration);

        // services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZServer.API", Version = "v1" }); });

        services.AddCors(option =>
        {
            option
                .AddPolicy("cors", policy =>
                    policy.AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition", "X-Suggested-Filename")
                        .AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
                );
        });
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.Configure<PermissionOptions>(configuration);
        services.TryAddSingleton<IPermissionService, PermissionService>();

        if (_enableAuthorization)
        {
            var apiName = configuration["ApiName"];
            if (string.IsNullOrWhiteSpace(apiName))
            {
                apiName = "zserver-api";
            }

            // 认证
            var authenticationBuilder = services.AddAuthentication(x =>
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
                        ValidateAudience =
                            "true".Equals(configuration["JwtBearer:ValidateAudience"],
                                StringComparison.OrdinalIgnoreCase),
                        ValidateIssuer = "true".Equals(configuration["JwtBearer:ValidateIssuer"],
                            StringComparison.OrdinalIgnoreCase),
                        ValidIssuer = configuration["JwtBearer:ValidIssuer"],
                        ValidAudience = configuration["JwtBearer:ValidAudience"],
                        ValidateLifetime = "true".Equals(configuration["JwtBearer:ValidateLifetime"],
                            StringComparison.OrdinalIgnoreCase)
                    };

                    var keyType = configuration["JwtBearer:Key:kty"];
                    if (!string.IsNullOrEmpty(keyType))
                    {
                        var webKey = configuration.GetSection("JwtBearer:Key").Get<JsonWebKey>();
                        var rsaParameters = new RSAParameters
                        {
                            Modulus = Base64UrlEncoder.DecodeBytes(webKey.N),
                            Exponent = Base64UrlEncoder.DecodeBytes(webKey.E),
                            D = string.IsNullOrEmpty(webKey.D) ? null : Base64UrlEncoder.DecodeBytes(webKey.D),
                            P = string.IsNullOrEmpty(webKey.P) ? null : Base64UrlEncoder.DecodeBytes(webKey.P),
                            Q = string.IsNullOrEmpty(webKey.Q) ? null : Base64UrlEncoder.DecodeBytes(webKey.Q),
                            DP = string.IsNullOrEmpty(webKey.DP) ? null : Base64UrlEncoder.DecodeBytes(webKey.DP),
                            DQ = string.IsNullOrEmpty(webKey.DQ) ? null : Base64UrlEncoder.DecodeBytes(webKey.DQ),
                            InverseQ = string.IsNullOrEmpty(webKey.QI) ? null : Base64UrlEncoder.DecodeBytes(webKey.QI)
                        };

                        // 可选：禁用自动发现配置的额外校验
                        options.ConfigurationManager = null;
                        options.Authority = null;
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsaParameters);
                    }
                    else
                    {
                        options.Authority = configuration["JwtBearer:Authority"] ??
                                            throw new ApplicationException("JwtBearer:Authority is null or empty. ");

                        options.RequireHttpsMetadata = "true".Equals(configuration["JwtBearer:RequireHttpsMetadata"],
                            StringComparison.OrdinalIgnoreCase);
                        options.MetadataAddress = configuration["JwtBearer:MetadataAddress"] ?? string.Empty;

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
            var tokens = configuration
                .GetSection("tokens").Get<HashSet<string>>();
            authenticationBuilder.AddScheme<TokenAuthOptions, TokenAuthHandler>("Token",
                opts =>
                {
                    opts.Tokens = tokens;
                    opts.ApiName = apiName;
                });
            // 授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "Token");
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", apiName);
                });
                options.AddPolicy("api-document", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "Token");
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("zserver-api-document");
                });
            });
        }
        else
        {
            // 当授权被禁用时，添加一个“空”的授权服务或什么都不做。
            // 为了确保 MVC 能正常运行，我们可以添加一个允许所有请求的授权策略到全局过滤器。
            // 但更优雅的方式是通过自定义过滤器处理（见下一步）。
            services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
        }

        services.AddHealthChecks();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        Log.SetLoggerFactory(loggerFactory);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHealthChecks("/healthz");
        // app.UseResponseCompression();
        app.UseResponseCaching();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();

        app.UseCors("cors");
        app.Use((context, next) =>
        {
            LogContext.Push(new WithExtraEnricher(context));
            return next.Invoke();
        });
        app.UseEndpoints(endpoints =>
        {
#if DEBUG
            endpoints.MapOpenApi();
#else
            endpoints.MapOpenApi()
                .RequireAuthorization("api-document");
#endif

            var endpointConventionBuilder = endpoints.MapControllers();
            if (_enableAuthorization)
            {
                endpointConventionBuilder.RequireAuthorization("default");
            }
        });
    }
}
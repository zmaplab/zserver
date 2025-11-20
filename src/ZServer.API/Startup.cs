using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.IO.Converters;
using Orleans.Configuration;
using Serilog.Context;
using ZMap.Permission;
using ZServer.API.Authentication;
using ZServer.API.Filters;
using ZServer.Store;
using Log = ZMap.Infrastructure.Log;

namespace ZServer.API;

public class Startup(IConfiguration configuration)
{
    private bool _enableAuthorization;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        _enableAuthorization = configuration.GetValue<bool>("EnableAuthorization");

        // 替换默认的 IHttpRequestIdentifierFeature
        services.AddTransient<IHttpRequestIdentifierFeature>(sp =>
        {
            // 获取原始 Feature（框架默认实现）
            var originalFeature = sp.GetRequiredService<HttpRequestIdentifierFeature>();
            // 注入依赖项
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            // 返回自定义 Feature
            return new CustomTraceIdentifierFeature(originalFeature, httpContextAccessor);
        });

        services.AddControllers(x => { x.Filters.Add<GlobalExceptionFilter>(); })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
            });

        services.AddResponseCaching();
        services.AddRouting(x => { x.LowercaseUrls = true; });

        var configProvider = Environment.GetEnvironmentVariable("ZSERVER_CONFIG_PROVIDER");
        configProvider = string.IsNullOrEmpty(configProvider) ? "File" : "SocoDB";
        var configAddr = Environment.GetEnvironmentVariable("ZSERVER_CONFIG_ADDR");
        configAddr = string.IsNullOrEmpty(configAddr) ? "conf/zserver.json" : configAddr;
        switch (configProvider.ToLower())
        {
            case "file":
                services.AddZServer(configuration, configAddr).AddSkiaSharpRenderer();
                break;
            case "socodb":
                services.AddZServer(configuration).AddSkiaSharpRenderer();
                services.AddSingleton<IJsonStoreProvider>(provider =>
                    new SocoStoreProvider(configAddr, provider.GetRequiredService<IHttpClientFactory>(),
                        provider.GetRequiredService<ILogger<SocoStoreProvider>>()));
                break;
            default:
                throw new NotSupportedException($"不支持的配置提供者 {configProvider}");
        }

        services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });
        services.Configure<ServerOptions>(configuration);
        services.Configure<ClusterOptions>("Orleans", configuration);

        // services.AddOrleansClient(builder =>
        // {
        //     if ("true".Equals(Configuration["standalone"]))
        //     {
        //         builder
        //             .UseLocalhostClustering(30000, "zserver", "zserver");
        //     }
        //     else
        //     {
        //         builder.Configure<ClusterOptions>(options =>
        //         {
        //             options.ClusterId = Configuration["Orleans:ClusterId"];
        //             options.ServiceId = Configuration["Orleans:ServiceId"];
        //         });
        //
        //         builder.UseAdoNetClustering(options =>
        //         {
        //             options.ConnectionString = Configuration["Orleans:ConnectionString"];
        //             options.Invariant = Configuration["Orleans:Invariant"];
        //         });
        //     }
        // });

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
        services.AddSingleton<IPermissionService, PermissionService>();

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
                    options.Authority = configuration["jwtBearer:authority"];
                    options.RequireHttpsMetadata = configuration["jwtBearer:requireHttpsMetadata"] == "true";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience =
                            "true".Equals(configuration["jwtBearer:validateAudience"],
                                StringComparison.OrdinalIgnoreCase),
                        ValidateIssuer = "true".Equals(configuration["jwtBearer:validateIssuer"],
                            StringComparison.OrdinalIgnoreCase)
                    };
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
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        Log.SetLoggerFactory(loggerFactory);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // if (configuration["Swagger"]?.ToLower() == "true")
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZServer API v1"));
        // }

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
            var endpointConventionBuilder = endpoints.MapControllers();
            if (_enableAuthorization)
            {
                endpointConventionBuilder.RequireAuthorization("default");
            }
        });
    }
}
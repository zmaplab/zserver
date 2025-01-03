using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.IO.Converters;
using Orleans.Configuration;
using Serilog.Context;
using ZMap.Permission;
using ZMap.Source.CloudOptimizedGeoTIFF;
using ZServer.API.Filters;
using Log = ZMap.Infrastructure.Log;

namespace ZServer.API;

public class Startup(IConfiguration configuration)
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(x => x.Filters.Add<GlobalExceptionFilter>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
            });

        // 网关层处理
        // services.AddResponseCompression(options =>
        // {
        //     options.Providers.Add<BrotliCompressionProvider>();
        //     options.Providers.Add<GzipCompressionProvider>();
        //     options.MimeTypes =
        //         ResponseCompressionDefaults.MimeTypes.Concat(
        //             new[] { "image/svg+xml", "db" });
        // });

        services.AddResponseCaching();
        services.AddRouting(x => { x.LowercaseUrls = true; });

        services.AddZServer(configuration, "conf/zserver.json").AddSkiaSharpRenderer();
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

        if ("true".Equals(configuration["EnableAuthorization"], StringComparison.OrdinalIgnoreCase))
        {
            // 认证
            services.AddAuthentication(x =>
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
                    options.Authority = configuration["Authority"];
                    options.RequireHttpsMetadata = configuration["RequireHttpsMetadata"] == "true";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false, ValidateIssuer = false
                    };
                });
            // 授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy("JWT", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", configuration["ApiName"] ?? "zserver-api");
                });
            });
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

        var enableAuthorization =
            "true".Equals(configuration["EnableAuthorization"], StringComparison.OrdinalIgnoreCase);
        if (enableAuthorization)
        {
            app.UseAuthorization();
            app.UseAuthentication();
        }

        app.UseCors("cors");
        app.Use((context, next) =>
        {
            LogContext.Push(new WithExtraEnricher(context));
            return next.Invoke();
        });
        app.UseEndpoints(endpoints =>
        {
            var endpointConventionBuilder = endpoints.MapControllers();
            if (enableAuthorization)
            {
                endpointConventionBuilder.RequireAuthorization("JWT");
            }
        });
    }
}
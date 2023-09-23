using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite.IO.Converters;
using Orleans.Configuration;
using ZServer.API.Filters;
using Log = ZMap.Infrastructure.Log;

namespace ZServer.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(x => x.Filters.Add<GlobalExceptionFilter>())
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
                });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "image/svg+xml", "db" });
            });
            services.AddResponseCaching();
            services.AddRouting(x => { x.LowercaseUrls = true; });

            services.AddZServer(Configuration, "conf/zserver.json").AddSkiaSharpRenderer();
            services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });

            services.Configure<ClusterOptions>("Orleans", Configuration);
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZServer.API", Version = "v1" });
            });

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log.Logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ZServer");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Configuration["Swagger"]?.ToLower() == "true")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZServer API v1"));
            }

            app.UseResponseCompression();
            app.UseResponseCaching();

            app.UseRouting();
            app.UseCors("cors");
            app.UseMiddleware<LoggerMiddleware>();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
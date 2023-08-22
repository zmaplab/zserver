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
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
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

            services.AddZServer(Configuration,"conf/zserver.json").AddSkiaSharpRenderer();
            services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });

            var client = new Lazy<IClusterClient>(() =>
            {
                if ("true".Equals(Configuration["standalone"]))
                {
                    var cb = new ClientBuilder()
                        .UseLocalhostClustering(30000, "zserver", "zserver")
                        .ConfigureLogging(logging => logging.AddSerilog())
                        .Build();
                    cb.Connect().GetAwaiter().GetResult();
                    return cb;
                }
                else
                {
                    var cb = new ClientBuilder()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = Configuration["Orleans:ClusterId"];
                            options.ServiceId = Configuration["Orleans:ServiceId"];
                        })
                        .UseAdoNetClustering(options =>
                        {
                            options.ConnectionString = Configuration["Orleans:ConnectionString"];
                            options.Invariant = Configuration["Orleans:Invariant"];
                        })
                        .ConfigureLogging(logging => logging.AddSerilog())
                        .Build();
                    cb.Connect().GetAwaiter().GetResult();
                    return cb;
                }
            });

            services.Configure<ClusterOptions>("Orleans", Configuration);
            services.AddSingleton(client);

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

            // BridgeDbContext dbContext = new BridgeDbContext(
            //     "User ID=postgres;Password=oVkr7GiT29CAkw;Host=hdy.dev;Port=1921;Database=szsf_dev;Pooling=true;",
            //     "szsf_dev", "xiao_hui");
            // dbContext.GetType().GetMethod("Set", BindingFlags.Public)
            //     .Invoke(dbContext, new object[] {   });

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
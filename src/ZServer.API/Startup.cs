// using System;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Log = ZMap.Infrastructure.Log;
//
// namespace ZServer.API;
//
// /// <summary>
// /// 
// /// </summary>
// /// <param name="configuration"></param>
// public class Startup(IConfiguration configuration)
// {
//     private bool _enableAuthorization;
//
//     // This method gets called by the runtime. Use this method to add services to the container.
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="services"></param>
//     /// <exception cref="ApplicationException"></exception>
//     public void ConfigureServices(IServiceCollection services)
//     {
//         
//     }
//
//     // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="app"></param>
//     /// <param name="env"></param>
//     public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//     {
//         var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
//         Log.SetLoggerFactory(loggerFactory);
//
//         if (env.IsDevelopment())
//         {
//             app.UseDeveloperExceptionPage();
//         }
//
//         app.UseHealthChecks("/healthz");
//         // app.UseResponseCompression();
//         app.UseResponseCaching();
//
//         app.UseRouting();
//
// // 先认证
//         app.UseAuthentication();
// // 后授权
//         app.UseAuthorization();
//
//         app.UseCors("cors");
//         app.UseEndpoints(endpoints =>
//         {
// #if DEBUG
//             endpoints.MapOpenApi();
// #else
//             endpoints.MapOpenApi()
//                 .RequireAuthorization("api-document");
// #endif
//
//             var endpointConventionBuilder = endpoints.MapControllers();
//             if (_enableAuthorization)
//             {
//                 endpointConventionBuilder.RequireAuthorization("default");
//             }
//         });
//     }
// }
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using Serilog.Context;
// using ZMap;
//
// namespace ZServer.API;
//
// public class LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
// {
//     public async Task Invoke(HttpContext context)
//     {
//         var hostIp = EnvironmentVariables.HostIP;
//         if (!string.IsNullOrWhiteSpace(hostIp))
//         {
//             LogContext.PushProperty("HOST_IP", hostIp); //traceId
//         }
//
//         if (context.Request.Headers.TryGetValue("trace-id", out var header))
//         {
//             LogContext.PushProperty("TraceId", header.ToString()); //traceId
//         }
//         else
//         {
//             context.Request.Headers.TryAdd("trace-id", context.TraceIdentifier);
//             LogContext.PushProperty("TraceId", context.TraceIdentifier); //traceId
//         }
//
//         await next(context);
//     }
// }
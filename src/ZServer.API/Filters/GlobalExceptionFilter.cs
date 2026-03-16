using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZServer.API.Filters;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment hostEnvironment)
    : IExceptionFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void OnException(ExceptionContext context)
    {
        context.Result = new JsonResult(new
        {
            success = false,
            msg = "系统内部错误",
            code = 500
        });
        context.ExceptionHandled = true;
        logger.LogError(context.Exception, "请求异常");

        var statusCode = context.Exception switch
        {
            ArgumentException => 400,
            UnauthorizedAccessException => 401,
            _ => 500
        };

        context.HttpContext.Response.StatusCode = statusCode;
        context.Result = new JsonResult(new
        {
            success = false,
            msg = hostEnvironment.IsDevelopment() ? context.Exception.Message : "请求处理失败",
            code = statusCode,
            traceId = context.HttpContext.TraceIdentifier
        });

        logger.LogError(context.Exception, "请求异常 TraceId: {TraceId}",
            context.HttpContext.TraceIdentifier);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ZServer.API.Filters;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void OnException(ExceptionContext context)
    {
        context.HttpContext.Response.StatusCode = 500;
        context.Result = new JsonResult(new
        {
            success = false,
            msg = "系统内部错误",
            code = 500
        });
        context.ExceptionHandled = true;
        logger.LogError(context.Exception, "请求异常");
    }
}
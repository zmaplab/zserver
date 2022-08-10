using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ZServer.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        context.HttpContext.Response.StatusCode = 500;
        context.Result = new JsonResult(new
        {
            success = false,
            msg = "系统内部错误",
            code = 500
        });

        _logger.LogError(context.Exception.ToString());
    }
}
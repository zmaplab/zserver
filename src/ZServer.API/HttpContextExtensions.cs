using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ZServer.Interfaces;

namespace ZServer.API;

public static class HttpContextExtensions
{
    public static async Task WriteZServerResponseAsync(this HttpContext httpContext, ZServerResponse result,
        string infoFormat = "text/xml")
    {
        if (result.Exception == null)
        {
            httpContext.Response.ContentType = result.ContentType;
            httpContext.Response.ContentLength = result.Body.Length;
            await httpContext.Response.BodyWriter.WriteAsync(result.Body);
            await httpContext.Response.BodyWriter.FlushAsync();
        }
        else
        {
            var exceptionReport = new ServerExceptionReport
            {
                Exceptions = new List<ServerException>
                {
                    new()
                    {
                        Code = result.Exception.Code,
                        Locator = result.Exception.Locator,
                        Text = result.Exception.Text
                    }
                }
            };

            var bytes = exceptionReport.Serialize(infoFormat);
            httpContext.Response.ContentType = infoFormat;
            httpContext.Response.ContentLength = bytes.Length;
            await httpContext.Response.BodyWriter.WriteAsync(bytes);
            await httpContext.Response.BodyWriter.FlushAsync();
        }
    }

    public static async Task WriteAsync(this HttpContext httpContext, object result)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
        var bytes = JsonSerializer.SerializeToUtf8Bytes(result, options.JsonSerializerOptions);
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.ContentLength = bytes.Length;
        await httpContext.Response.BodyWriter.WriteAsync(bytes);
        await httpContext.Response.BodyWriter.FlushAsync();
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.IO.Converters;
using ZServer.Interfaces;

namespace ZServer.API
{
    public static class HttpContextExtensions
    {
        private static readonly JsonSerializerOptions Options = new();

        static HttpContextExtensions()
        {
            Options.Converters.Add(new GeoJsonConverterFactory());
        }

        public static async Task WriteResultAsync(this HttpContext httpContext, string code, string message,
            Stream stream, string imageFormat,
            string infoFormat = "text/xml")
        {
            if (string.IsNullOrEmpty(message))
            {
                httpContext.Response.ContentType = imageFormat;
                httpContext.Response.ContentLength = stream.Length;
                await stream.CopyToAsync(httpContext.Response.Body);
            }
            else
            {
                var exceptionReport = new ServerExceptionReport
                {
                    Exceptions = new List<ServerException>
                    {
                        new()
                        {
                            Code = code,
                            Locator = null,
                            Text = message
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

        public static async Task WriteResultAsync(this HttpContext httpContext, MapResult result,
            string infoFormat = "text/xml")
        {
            if (string.IsNullOrEmpty(result.Code) || result.Code == "200")
            {
                httpContext.Response.ContentType = result.ImageType;
                httpContext.Response.ContentLength = result.ImageBytes.Length;
                await httpContext.Response.BodyWriter.WriteAsync(result.ImageBytes);
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
                            Code = result.Code,
                            Locator = result.Locator,
                            Text = result.Message
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
            var bytes = JsonSerializer.SerializeToUtf8Bytes(result, Options);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.ContentLength = bytes.Length;
            await httpContext.Response.BodyWriter.WriteAsync(bytes);
            await httpContext.Response.BodyWriter.FlushAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using ProjNet.IO.CoordinateSystems;
using ZMap.Infrastructure;

namespace ZServer.API.Controllers;

[ApiController]
[Route("api/v1.0/tools")]
[ZServerAuthorize]
public class ToolController(ILogger<ToolController> logger, IMemoryCache memoryCache) : ControllerBase
{
    [HttpPost("findAuthority")]
    public async Task FindAuthority()
    {
        using var streamReader = new StreamReader(HttpContext.Request.Body);
        var wkt = await streamReader.ReadToEndAsync();
        var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(wkt)));
        var output = memoryCache.GetOrCreate($"ZS_CRS_AUTHORITY_{hash}", entry =>
        {
            ApiResult<string> result = null;
            try
            {
                var cs1 = (CoordinateSystem)CoordinateSystemWktReader.Parse(wkt);
                if (!string.IsNullOrEmpty(cs1.Authority) && cs1.AuthorityCode > 0)
                {
                    result = new ApiResult<string>
                    {
                        Success = true,
                        Code = 0,
                        Data = $"{cs1.Authority}:{cs1.AuthorityCode}",
                        Msg = string.Empty
                    };
                }
                else
                {
                    foreach (var kv in CoordinateReferenceSystem.SRIDCache)
                    {
                        if (!CoordinateSystemComparer.AreEqual(cs1, kv.Value))
                        {
                            continue;
                        }

                        result = new ApiResult<string>
                        {
                            Success = true,
                            Code = 0,
                            Data = $"{kv.Value.Authority}:{kv.Value.AuthorityCode}",
                            Msg = string.Empty
                        };
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "{WKT} is not a valid WKT", wkt);
                result = new ApiResult<string>
                {
                    Success = false,
                    Code = -1,
                    Data = string.Empty,
                    Msg = "WKT 解析失败"
                };
            }

            result ??= new ApiResult<string>
            {
                Success = false,
                Code = 404,
                Data = string.Empty,
                Msg = "疑似非标准 CRS"
            };

            entry.SetValue(result);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return result;
        });

        await HttpContext.WriteAsync(output);
    }
}
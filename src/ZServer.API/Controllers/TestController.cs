using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZMap;
using ZMap.Extensions;

namespace ZServer.API.Controllers;
#if DEBUG
/// <summary>
/// 
/// </summary>
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    [HttpGet]
    public async Task GetAsync()
    {
        var assembly = typeof(Layer).Assembly;
        var name = assembly.GetManifestResourceNames().First(x => x.EndsWith("proj.xml"));
        await using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
        {
            return;
        }

        HttpContext.Response.ContentType = "application/xml";
        HttpContext.Response.ContentLength = stream.Length;
        await HttpContext.Response.BodyWriter.WriteAsync(await stream.ToArrayAsync());
        await HttpContext.Response.BodyWriter.FlushAsync();
    }
}
#endif
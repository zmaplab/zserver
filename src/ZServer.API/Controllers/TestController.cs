using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ZServer.API.Controllers;
#if DEBUG
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task GetAsync()
    {
            var xml = await System.IO.File.ReadAllBytesAsync("proj.xml");
            // return new ContentResult
            // {
            //     Content = xml,
            //     ContentType = "application/xml"
            // };
            HttpContext.Response.ContentType = "application/xml";
            HttpContext.Response.ContentLength = xml.Length;
            await HttpContext.Response.BodyWriter.WriteAsync(xml);
            await HttpContext.Response.BodyWriter.FlushAsync();
        }
}
#endif
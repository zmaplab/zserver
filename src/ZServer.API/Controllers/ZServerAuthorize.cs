// using System;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace ZServer.API.Controllers;
//
// [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
// public class ZServerAuthorize : Attribute, IAsyncAuthorizationFilter
// {
//     public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
//     {
//         var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
//         if (!"true".Equals(configuration["EnableAuthorization"], StringComparison.OrdinalIgnoreCase))
//         {
//             return;
//         }
//
//         var authorizationService = context.HttpContext.RequestServices.GetService<IAuthenticationService>();
//         var authorizationResult =
//             await authorizationService.AuthenticateAsync(context.HttpContext, JwtBearerDefaults.AuthenticationScheme);
//         if (!authorizationResult.Succeeded)
//         {
//             context.Result = new ForbidResult();
//         }
//     }
// }
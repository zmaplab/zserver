using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace ZServer.API.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class ZServerAuthorize : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var jwtBearerHandler = context.HttpContext.RequestServices.GetService<JwtBearerHandler>();
        if (jwtBearerHandler == null)
        {
            return;
        }

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthenticationService>();
        var authorizationResult = await authorizationService.AuthenticateAsync(context.HttpContext, JwtBearerDefaults.AuthenticationScheme);
        if (!authorizationResult.Succeeded)
        {
            context.Result = new ForbidResult();
        }
    }
}
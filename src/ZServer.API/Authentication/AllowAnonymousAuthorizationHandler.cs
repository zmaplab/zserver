using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ZServer.API.Authentication;

// 可选：一个空的 IAuthorizationHandler，当禁用授权时用于满足依赖注入需求
/// <summary>
/// 
/// </summary>
public class AllowAnonymousAuthorizationHandler : IAuthorizationHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        // 直接让所有要求都通过
        foreach (var requirement in context.Requirements)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
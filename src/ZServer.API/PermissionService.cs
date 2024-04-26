using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZMap.Permission;

namespace ZServer.API;

public class PermissionService(
    IOptionsMonitor<PermissionOptions> optionsMonitor,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PermissionService> logger,
    IHttpClientFactory clientFactory,
    IMemoryCache memoryCache) : IPermissionService
{
    public async ValueTask<bool> EnforceAsync(string action, string resource,
        PolicyEffect policyEffect)
    {
        var api = optionsMonitor.CurrentValue.PermissionApi;

        // 未配置权限API， 直接放行
        if (string.IsNullOrEmpty(api))
        {
            return true;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var token = await httpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var key = $"{userId}:{action}:{resource}:{policyEffect.Name}";
        var value = await memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, api);
            var content = new StringContent(JsonSerializer.Serialize(new[]
            {
                new
                {
                    Action = action,
                    Resource = resource,
                    PolicyEffect = policyEffect.Name
                }
            }), Encoding.UTF8);
            request.Headers.TryAddWithoutValidation("Internal-Caller", "true");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            request.Content = content;
            var client = clientFactory.CreateClient("PermissionService");
            var response = await client.SendAsync(request);
            // var json = await response.Content.ReadAsStringAsync();

            var result = await JsonSerializer.DeserializeAsync<bool[]>(await response.Content.ReadAsStreamAsync());
            var permission = result.ElementAtOrDefault(0);
            entry.SetValue(permission);
            entry.SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(10));
            return permission;
        });

        if (!value)
        {
            logger.LogWarning(
                "鉴权失败 资源: {Resource}, 操作: {Action}, 效果: {Effect}", resource, action, policyEffect);
        }
        else
        {
            logger.LogDebug(
                "鉴权成功, 资源: {Resource}, 操作: {Action}, 效果: {Effect}", resource, action, policyEffect);
        }

        return value;
    }
}
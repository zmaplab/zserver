namespace ZMap.Permission;

public interface IPermissionService
{
    ValueTask<bool> EnforceAsync(string action, string resource, PolicyEffect policyEffect);
}
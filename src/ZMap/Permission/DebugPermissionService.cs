namespace ZMap.Permission;

public class DebugPermissionService
    : IPermissionService
{
    public ValueTask<bool> EnforceAsync(string action, string resource, PolicyEffect policyEffect)
    {
        return new ValueTask<bool>(true);
    }
}
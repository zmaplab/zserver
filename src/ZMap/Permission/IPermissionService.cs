using System.Threading.Tasks;

namespace ZMap.Permission;

public interface IPermissionService
{
    Task<bool> EnforceAsync(string action, string resource, PolicyEffect policyEffect);
}
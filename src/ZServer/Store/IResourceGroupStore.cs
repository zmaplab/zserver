using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap;
using ZMap.Store;

namespace ZServer.Store;

/// <summary>
/// 
/// </summary>
public interface IResourceGroupStore : IRefresher
{
    /// <summary>
    /// 通过名称查找工作区
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    ValueTask<ResourceGroup> FindAsync(string name);

    /// <summary>
    /// 查询所有资源组
    /// </summary>
    /// <returns></returns>
    ValueTask<List<ResourceGroup>> GetAllAsync();
}
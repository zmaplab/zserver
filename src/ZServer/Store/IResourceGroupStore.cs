using System.Collections.Generic;
using System.Threading.Tasks;
using ZServer.Entity;

namespace ZServer.Store
{
    /// <summary>
    /// 
    /// </summary>
    public interface IResourceGroupStore
    {
        /// <summary>
        /// 通过名称查找工作区
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<ResourceGroup> FindAsync(string name);

        /// <summary>
        /// 查询所有资源组
        /// </summary>
        /// <returns></returns>
        Task<List<ResourceGroup>> GetAllAsync();
    }
}
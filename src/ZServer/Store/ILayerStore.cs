using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap;

namespace ZServer.Store
{
    /// <summary>
    /// Layer 配置存储器
    /// </summary>
    public interface ILayerStore : IRefresher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceGroupName"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        Task<ILayer> FindAsync(string resourceGroupName, string layerName);

        // Task<ILayer> FindAsync(string layerName);

        /// <summary>
        /// 查询所有图层配置
        /// </summary>
        /// <returns></returns>
        Task<List<ILayer>> GetAllAsync();

        /// <summary>
        /// 查询所有图层配置
        /// </summary>
        /// <returns></returns>
        Task<List<ILayer>> GetListAsync(string resourceGroupName);
    }
}
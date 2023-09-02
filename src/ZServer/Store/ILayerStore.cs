using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap;
using ZMap.Store;

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
        Task<Layer> FindAsync(string resourceGroupName, string layerName);

        // Task<ILayer> FindAsync(string layerName);

        /// <summary>
        /// 查询所有图层配置
        /// </summary>
        /// <returns></returns>
        Task<List<Layer>> GetAllAsync();

        /// <summary>
        /// 查询所有图层配置
        /// </summary>
        /// <returns></returns>
        Task<List<Layer>> GetListAsync(string resourceGroupName);
    }
}
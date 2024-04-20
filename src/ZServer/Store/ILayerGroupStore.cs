using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap;
using ZMap.Store;

namespace ZServer.Store;

public interface ILayerGroupStore : IRefresher
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceGroup"></param>
    /// <param name="layerGroupName"></param>
    /// <returns></returns>
    Task<LayerGroup> FindAsync(string resourceGroup, string layerGroupName);

    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="layerGroupName"></param>
    // /// <returns></returns>
    // Task<LayerGroup> FindAsync(string layerGroupName);

    /// <summary>
    /// 查询所有图层配置
    /// </summary>
    /// <returns></returns>
    Task<List<LayerGroup>> GetAllAsync();

    /// <summary>
    /// 查询所有图层配置
    /// </summary>
    /// <returns></returns>
    Task<List<LayerGroup>> GetListAsync(string resourceGroup);
}
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace ZMap.Source;

/// <summary>
/// 矢量数据源
/// </summary>
public interface IVectorSource : ISource
{
    /// <summary>
    /// 设置过滤器
    /// </summary>
    string Filter { get; set; }

    /// <summary>
    /// 获取指定区域的图斑
    /// TODO: 后面要转成表达式， 来解析成 SQL Expression<Func<Feature, bool>> filter 
    /// </summary>
    /// <param name="extent">区域</param>
    /// <returns></returns>
    Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent);
}
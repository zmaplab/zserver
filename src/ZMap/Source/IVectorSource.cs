using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZMap.Source
{
    /// <summary>
    /// 矢量数据源
    /// </summary>
    public interface IVectorSource : ISource
    {
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="filter"></param>
        void SetFilter(IFeatureFilter filter);

        IFeatureFilter Filter { get; }

        /// <summary>
        /// 获取指定区域的图斑
        /// TODO: 后面要转成表达式， 来解析成 SQL Expression<Func<Feature, bool>> filter 
        /// </summary>
        /// <param name="extent">区域</param>
        /// <returns></returns>
        ValueTask<IEnumerable<Feature>> GetFeaturesInExtentAsync(NetTopologySuite.Geometries.Envelope extent);
    }
}
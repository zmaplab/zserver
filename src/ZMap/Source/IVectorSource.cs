namespace ZMap.Source;

/// <summary>
/// 矢量数据源
/// </summary>
public interface IVectorSource : ISource
{
    /// <summary>
    /// 获取指定区域的图斑
    /// </summary>
    /// <param name="extent">区域</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IEnumerable<Feature>> GetFeaturesAsync(Envelope extent, string filter);
}
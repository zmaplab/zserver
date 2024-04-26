using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace ZMap.Source;

public abstract class VectorSourceBase : IVectorSource
{
    /// <summary>
    /// 数据源名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 空间标识符
    /// </summary>
    public int Srid { get; set; }

    /// <summary>
    /// 过滤器
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// 查询指定范围内的要素
    /// </summary>
    /// <param name="extent"></param>
    /// <returns></returns>
    public abstract Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent);

    /// <summary>
    /// 数据源覆盖范围
    /// </summary>
    /// <returns></returns>
    public abstract Envelope GetEnvelope();

    /// <summary>
    /// 复制数据源对象
    /// </summary>
    /// <returns></returns>
    public abstract ISource Clone();

    /// <summary>
    /// 释放
    /// </summary>
    public abstract void Dispose();
}
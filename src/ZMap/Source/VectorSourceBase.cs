namespace ZMap.Source;

public abstract class VectorSourceBase : IVectorSource
{
    public string Key { get; set; }

    /// <summary>
    /// 数据源名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 空间标识符
    /// </summary>
    public int Srid { get; set; }

    /// <summary>
    /// 投影系统
    /// </summary>
    public CoordinateSystem CoordinateSystem { get; set; }

    /// <summary>
    /// 查询指定范围内的要素
    /// </summary>
    /// <param name="extent"></param>
    /// <param name="fitler"></param>
    /// <returns></returns>
    public abstract Task<IEnumerable<Feature>> GetFeaturesAsync(Envelope extent, string fitler);

    /// <summary>
    /// 数据源覆盖范围
    /// </summary>
    /// <returns></returns>
    public abstract Envelope Envelope { get; }

    /// <summary>
    /// 复制数据源对象
    /// </summary>
    /// <returns></returns>
    public abstract ISource Clone();

    /// <summary>
    /// 释放
    /// </summary>
    public abstract void Dispose();

    public abstract Task LoadAsync();
}
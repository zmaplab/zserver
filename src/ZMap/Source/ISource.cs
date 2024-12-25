namespace ZMap.Source;

/// <summary>
/// TODO: 做成无状态类
/// </summary>
public interface ISource : IDisposable
{
    string Key { get; set; }

    /// <summary>
    /// 数据源名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 空间标识符
    /// </summary>
    int Srid { get; }

    /// <summary>
    /// 投影
    /// </summary>
    CoordinateSystem CoordinateSystem { get; }

    /// <summary>
    /// 数据源覆盖范围
    /// </summary>
    /// <returns></returns>
    Envelope Envelope { get; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    Task LoadAsync();

    /// <summary>
    /// 复制数据源对象
    /// </summary>
    /// <returns></returns>
    ISource Clone();
}
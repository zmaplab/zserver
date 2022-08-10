using System;

namespace ZMap.Source;

/// <summary>
/// 矢量过滤器
/// </summary>
public interface IFeatureFilter
{
    /// <summary>
    /// 编译成 PG 的 SQL
    /// </summary>
    /// <returns></returns>
    string GetSql();
    
    /// <summary>
    /// 编译成 Func，供内存数据过滤
    /// </summary>
    /// <returns></returns>
    Func<Feature, bool> GetFunc();
}
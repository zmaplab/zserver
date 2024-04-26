namespace ZMap;

/// <summary>
/// 渲染服务工厂
/// </summary>
public interface IGraphicsServiceProvider
{
    /// <summary>
    /// 创建渲染服务
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    IGraphicsService Create(int width, int height);
}
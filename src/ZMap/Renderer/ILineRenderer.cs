namespace ZMap.Renderer;

/// <summary>
/// 线绘制器
/// </summary>
/// <typeparam name="TGraphics"></typeparam>
public interface ILineRenderer<in TGraphics> : IVectorRenderer<TGraphics>;
namespace ZMap.Renderer;

/// <summary>
/// 符号绘制器
/// </summary>
/// <typeparam name="TGraphics"></typeparam>
public interface ISymbolRenderer<in TGraphics> : IVectorRenderer<TGraphics>;
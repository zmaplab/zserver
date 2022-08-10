// using System.Threading.Tasks;
// using Microsoft.Extensions.Caching.Memory;
// using ZMap.Source;
//
// namespace ZMap.Renderer
// {
//     public abstract class RendererBase<TTargetRenderContext, TStyle, TData> : IRenderer
//         where TTargetRenderContext : RenderContext where TStyle : Style.Style
//     {
//         protected IMemoryCache Cache { get; }
//
//         protected RendererBase(TStyle style, IMemoryCache cache)
//         {
//             Style = style;
//             Cache = cache;
//         }
//
//         protected virtual bool ShouldNotPaint(RenderContext context, TData data)
//         {
//             var shouldNotPaint = Style == null
//                                  || data == null
//                                  || context == null || context.IsEmpty || context is not TTargetRenderContext
//                                  || context.Viewport.Extent.Area == 0 || context.Viewport.Height == 0 ||
//                                  context.Viewport.Width == 0;
//             if (shouldNotPaint)
//             {
//                 return true;
//             }
//
//             return data is Feature { IsEmpty: true };
//         }
//
//         protected TStyle Style { get; }
//
//         public virtual async Task PaintAsync(RenderContext context, TData data)
//         {
//             if (ShouldNotPaint(context, data))
//             {
//                 return;
//             }
//
//             await PaintAsync((TTargetRenderContext)context, data);
//         }
//
//         protected abstract Task PaintAsync(TTargetRenderContext context, TData feature);
//     }
// }
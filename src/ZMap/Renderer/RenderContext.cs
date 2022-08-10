// using System;
//
// namespace ZMap.Renderer
// {
//     public abstract class RenderContext : IDisposable
//     {
//         protected RenderContext(Viewport viewport, IRendererFactory rendererFactory)
//         {
//             Viewport = viewport;
//             RendererFactory = rendererFactory;
//         }
//
//         public IRendererFactory RendererFactory { get; }
//         public Viewport Viewport { get; }
//         public abstract byte[] FlushImage();
//         public abstract void Dispose();
//         public abstract bool IsEmpty { get; }
//     }
// }
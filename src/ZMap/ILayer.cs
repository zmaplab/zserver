using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using ZMap.Source;
using ZMap.Style;

namespace ZMap
{
    /// <summary>
    /// 图层
    /// </summary>
    public interface ILayer : IVisibleLimit
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 栅格缓冲
        /// </summary>
        List<GridBuffer> Buffers { get; }

        /// <summary>
        /// TODO: 移动到矢量图层中，比如 TIFF 之类是没有 STYLE 的
        /// </summary>
        IReadOnlyCollection<StyleGroup> StyleGroups { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// 可渲染的范围
        /// </summary>
        Envelope Envelope { get; }

        /// <summary>
        /// 空间标识符
        /// </summary>
        int SRID { get; }

        ISource Source { get; }

        // Task PaintAsync(RenderContext context, string filter = null, string traceId = null);
        Task RenderAsync(IGraphicsService service, Viewport viewport, Zoom zoom, int targetSRID);

        void ClearEnvironments();
    }
}
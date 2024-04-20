using NetTopologySuite.Geometries;

namespace ZMap.Renderer;

public interface IVectorRenderer<in TGraphics> : IRenderer<TGraphics, Geometry>;
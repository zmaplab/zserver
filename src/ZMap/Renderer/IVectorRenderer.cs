using ZMap.Source;

namespace ZMap.Renderer
{
    public interface IVectorRenderer<in TGraphics> : IRenderer<TGraphics, Feature>
    {
    }
}
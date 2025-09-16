using System.Runtime.InteropServices;

namespace ZMap.Renderer.SkiaSharp.GlContexts.Wgl;

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
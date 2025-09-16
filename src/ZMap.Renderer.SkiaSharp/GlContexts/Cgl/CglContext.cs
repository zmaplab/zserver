using System;
using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.GlContexts.Cgl;

public class CglContext : GlContext
{
    private IntPtr _fContext;


    public CglContext()
    {
        var attributes = new[]
        {
            CGLPixelFormatAttribute.kCGLPFAOpenGLProfile,
            (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
            CGLPixelFormatAttribute.kCGLPFADoubleBuffer,
            CGLPixelFormatAttribute.kCGLPFANone
        };

        Cgl.CGLChoosePixelFormat(attributes, out var pixFormat, out _);

        if (pixFormat == IntPtr.Zero)
        {
            throw new Exception("CGLChoosePixelFormat failed.");
        }

        Cgl.CGLCreateContext(pixFormat, IntPtr.Zero, out _fContext);
        Cgl.CGLReleasePixelFormat(pixFormat);

        if (_fContext == IntPtr.Zero)
        {
            throw new Exception("CGLCreateContext failed.");
        }
    }

    public override void MakeCurrent()
    {
        Cgl.CGLSetCurrentContext(_fContext);
    }

    public override void SwapBuffers()
    {
        Cgl.CGLFlushDrawable(_fContext);
    }

    public override void Destroy()
    {
        if (_fContext != IntPtr.Zero)
        {
            Console.WriteLine("CGL context released: " + this.GetHashCode());
            Cgl.CGLReleaseContext(_fContext);
            _fContext = IntPtr.Zero;
        }
    }

    public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
    {
        var textures = new uint[1];
        Cgl.glGenTextures(textures.Length, textures);
        var textureId = textures[0];

        Cgl.glBindTexture(
            Cgl.GL_TEXTURE_2D, textureId);
        Cgl.glTexImage2D(
            Cgl.GL_TEXTURE_2D, 0,
            Cgl.GL_RGBA, textureSize.Width, textureSize.Height, 0,
            Cgl.GL_RGBA,
            Cgl.GL_UNSIGNED_BYTE, IntPtr.Zero);
        Cgl.glBindTexture(
            Cgl.GL_TEXTURE_2D, 0);

        return new GRGlTextureInfo
        {
            Id = textureId,
            Target = Cgl.GL_TEXTURE_2D,
            Format = Cgl.GL_RGBA8
        };
    }

    public override void DestroyTexture(uint texture)
    {
        Cgl.glDeleteTextures(1, new[] { texture });
    }
}
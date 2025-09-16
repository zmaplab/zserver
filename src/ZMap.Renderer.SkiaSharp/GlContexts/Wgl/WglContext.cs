using System;
using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.GlContexts.Wgl;

internal class WglContext : GlContext
{
    private static readonly object FLock = new();
    private static readonly Win32Window Window = new("WglContext");

    private IntPtr _pbufferHandle;
    private IntPtr _pbufferDeviceContextHandle;
    private IntPtr _pbufferGlContextHandle;

    public WglContext()
    {
        if (!Wgl.HasExtension(Window.DeviceContextHandle, "WGL_ARB_pixel_format") ||
            !Wgl.HasExtension(Window.DeviceContextHandle, "WGL_ARB_pbuffer"))
        {
            throw new Exception("DeviceContext does not have extensions.");
        }

        var iAttrs = new[]
        {
            Wgl.WGL_ACCELERATION_ARB, Wgl.WGL_FULL_ACCELERATION_ARB,
            Wgl.WGL_DRAW_TO_WINDOW_ARB, Wgl.TRUE,
            //Wgl.WGL_DOUBLE_BUFFER_ARB, (doubleBuffered ? TRUE : FALSE),
            Wgl.WGL_SUPPORT_OPENGL_ARB, Wgl.TRUE,
            Wgl.WGL_RED_BITS_ARB, 8,
            Wgl.WGL_GREEN_BITS_ARB, 8,
            Wgl.WGL_BLUE_BITS_ARB, 8,
            Wgl.WGL_ALPHA_BITS_ARB, 8,
            Wgl.WGL_STENCIL_BITS_ARB, 8,
            Wgl.NONE, Wgl.NONE
        };
        var piFormats = new int[1];
        uint nFormats;
        lock (FLock)
        {
            // HACK: This call seems to cause deadlocks on some systems.
            Wgl.wglChoosePixelFormatARB(Window.DeviceContextHandle, iAttrs, null, (uint)piFormats.Length, piFormats,
                out nFormats);
        }

        if (nFormats == 0)
        {
            Destroy();
            throw new Exception("Could not get pixel formats.");
        }

        _pbufferHandle = Wgl.wglCreatePbufferARB(Window.DeviceContextHandle, piFormats[0], 1, 1, null);
        if (_pbufferHandle == IntPtr.Zero)
        {
            Destroy();
            throw new Exception("Could not create Pbuffer.");
        }

        _pbufferDeviceContextHandle = Wgl.wglGetPbufferDCARB(_pbufferHandle);
        if (_pbufferDeviceContextHandle == IntPtr.Zero)
        {
            Destroy();
            throw new Exception("Could not get Pbuffer DC.");
        }

        var prevDeviceContext = Wgl.wglGetCurrentDC();
        var prevGlrc = Wgl.wglGetCurrentContext();

        _pbufferGlContextHandle = Wgl.wglCreateContext(_pbufferDeviceContextHandle);

        Wgl.wglMakeCurrent(prevDeviceContext, prevGlrc);

        if (_pbufferGlContextHandle == IntPtr.Zero)
        {
            Destroy();
            throw new Exception("Could not create PBuffer GL context.");
        }
    }

    public override void MakeCurrent()
    {
        if (!Wgl.wglMakeCurrent(_pbufferDeviceContextHandle, _pbufferGlContextHandle))
        {
            Destroy();
            throw new Exception("Could not set the context.");
        }
    }

    public override void SwapBuffers()
    {
        if (!Gdi32.SwapBuffers(_pbufferDeviceContextHandle))
        {
            Destroy();
            throw new Exception("Could not complete SwapBuffers.");
        }
    }

    public override void Destroy()
    {
        if (_pbufferGlContextHandle != IntPtr.Zero)
        {
            Wgl.wglDeleteContext(_pbufferGlContextHandle);
            _pbufferGlContextHandle = IntPtr.Zero;
        }

        if (_pbufferHandle == IntPtr.Zero)
        {
            return;
        }

        if (_pbufferDeviceContextHandle != IntPtr.Zero)
        {
            if (!Wgl.HasExtension(_pbufferDeviceContextHandle, "WGL_ARB_pbuffer"))
            {
                // ASSERT
            }

            Wgl.wglReleasePbufferDCARB?.Invoke(_pbufferHandle, _pbufferDeviceContextHandle);
            _pbufferDeviceContextHandle = IntPtr.Zero;
        }

        Wgl.wglDestroyPbufferARB?.Invoke(_pbufferHandle);
        _pbufferHandle = IntPtr.Zero;
    }

    public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
    {
        var textures = new uint[1];
        Wgl.glGenTextures(textures.Length, textures);
        var textureId = textures[0];

        Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, textureId);
        Wgl.glTexImage2D(Wgl.GL_TEXTURE_2D, 0, Wgl.GL_RGBA, textureSize.Width, textureSize.Height, 0, Wgl.GL_RGBA,
            Wgl.GL_UNSIGNED_BYTE, IntPtr.Zero);
        Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, 0);

        return new GRGlTextureInfo
        {
            Id = textureId,
            Target = Wgl.GL_TEXTURE_2D,
            Format = Wgl.GL_RGBA8
        };
    }

    public override void DestroyTexture(uint texture)
    {
        Wgl.glDeleteTextures(1, new[] { texture });
    }
}
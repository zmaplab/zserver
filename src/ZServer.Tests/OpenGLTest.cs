using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using SkiaSharp.Internals;
using Xunit;
using ZMap.Renderer.SkiaSharp.GlContexts;
using ZMap.Renderer.SkiaSharp.GlContexts.Cgl;

namespace ZServer.Tests;

public class OpenGLTest : BaseTests
{
    private static class Mac
    {
        private const string SystemLibrary = "/usr/lib/libSystem.dylib";

        private const int RTLD_LAZY = 1;
        private const int RTLD_NOW = 2;

        public static IntPtr dlopen(string path, bool lazy = true) =>
            dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

        [DllImport(SystemLibrary)]
        public static extern IntPtr dlopen(string path, int mode);

        [DllImport(SystemLibrary)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(SystemLibrary)]
        public static extern void dlclose(IntPtr handle);
    }

    public static T GetSymbolDelegate<T>(IntPtr library, string name)
        where T : Delegate
    {
        var symbol = GetSymbol(library, name);
        if (symbol == IntPtr.Zero)
            throw new EntryPointNotFoundException($"Unable to load symbol '{name}'.");

        return Marshal.GetDelegateForFunctionPointer<T>(symbol);
    }

    public static IntPtr GetSymbol(IntPtr library, string symbolName)
    {
        if (string.IsNullOrEmpty(symbolName))
            throw new ArgumentNullException(nameof(symbolName));

        return Mac.dlsym(library, symbolName);
    }

    public static void FreeLibrary(IntPtr library)
    {
        if (library == IntPtr.Zero)
            return;

        Mac.dlclose(library);
    }

    public static IntPtr LoadLibrary(string libraryName)
    {
        if (string.IsNullOrEmpty(libraryName))
            throw new ArgumentNullException(nameof(libraryName));

        IntPtr handle = Mac.dlopen(libraryName);
        return handle;
    }

    [Fact]
    public void Draw()
    {
        using var ctx = GlContextHelper.Create();
        // ctx.MakeCurrent();


        var grContext = GRContext.CreateGl();
        var lib = LoadLibrary("/System/Library/Frameworks/OpenGL.framework/Versions/A/Libraries/libGL.dylib");

        var glInterface = GRGlInterface.Create(name => { return GetSymbol(lib, name); });
        var gpuSurface = SKSurface.Create(grContext, false, new SKImageInfo(152, 146));
        var canvas = gpuSurface.Canvas;
        var image = SKImage.FromEncodedData("images/108.png");
        canvas.DrawImage(image, new SKPoint(0, 0));

        canvas.Flush();
        grContext.Flush();

        var skImage = gpuSurface.Snapshot();
        skImage.Encode(SKEncodedImageFormat.Png, 90).SaveTo(File.OpenWrite("OpenGLTest_Draw.png"));
    }
}
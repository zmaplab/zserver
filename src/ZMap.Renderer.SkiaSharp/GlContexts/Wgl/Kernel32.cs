using System;
using System.Runtime.InteropServices;

namespace ZMap.Renderer.SkiaSharp.GlContexts.Wgl;

public static class Kernel32
{
    private const string Kernel32FileName = "kernel32.dll";

    static Kernel32()
    {
        CurrentModuleHandle = GetModuleHandle(null);
        if (CurrentModuleHandle == IntPtr.Zero)
        {
            throw new Exception("Could not get module handle.");
        }
    }

    public static IntPtr CurrentModuleHandle { get; }

    [DllImport(Kernel32FileName, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr)] string lpModuleName);
}
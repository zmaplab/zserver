using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.Internals;
using ZMap.Renderer.SkiaSharp.GlContexts.Cgl;
using ZMap.Renderer.SkiaSharp.GlContexts.Glx;
using ZMap.Renderer.SkiaSharp.GlContexts.Wgl;

namespace ZMap.Renderer.SkiaSharp.GlContexts;

public static class GlContextHelper
{
    private static ILogger _logger;
    private static bool _isEnabled;

    private static readonly ThreadLocal<GlContext> ThreadLocalContext = new(() =>
    {
        if (PlatformConfiguration.IsLinux)
        {
            var ctx = new GlxContext();
            Console.WriteLine("CGL context created： " + ctx.GetHashCode());
            return ctx;
        }

        if (PlatformConfiguration.IsMac)
        {
            var ctx = new CglContext();
            Console.WriteLine("CGL context created： " + ctx.GetHashCode());
            return ctx;
        }

        if (PlatformConfiguration.IsWindows)
        {
            var ctx = new WglContext();
            Console.WriteLine("CGL context created： " + ctx.GetHashCode());
            return ctx;
        }

        Console.WriteLine("CGL context created null");
        return null;
    });

    public static GlContext Create()
    {
        return ThreadLocalContext.Value;

        // if (PlatformConfiguration.IsLinux)
        // {
        //     return new GlxContext();
        // }
        //
        // if (PlatformConfiguration.IsMac)
        // {
        //     return new CglContext();
        // }
        //
        // return PlatformConfiguration.IsWindows ? new WglContext() : null;
    }

    public static void Initialize()
    {
        try
        {
            _logger = Infrastructure.Log.CreateLogger(typeof(GlContextHelper));

            if (PlatformConfiguration.IsLinux)
            {
                using var ctx = new GlxContext();
                _isEnabled = true;
            }
            else if (PlatformConfiguration.IsMac)
            {
                using var ctx = new CglContext();
                // using var ctx1 = new CglContext();
                _isEnabled = true;
            }
            else if (PlatformConfiguration.IsWindows)
            {
                using var ctx = new WglContext();
                _isEnabled = true;
            }

            if (_isEnabled)
            {
                _logger.LogInformation("OpenGL 上下文初始化成功， 可以使用 GPU 加速。");
                // Console.WriteLine("OpenGL 上下文初始化成功， 可以使用 GPU 加速。");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "初始化 OpenGL 上下文失败， 无法使用 GPU 加速， 错误信息: {Message}", ex.Message);
        }
    }
}
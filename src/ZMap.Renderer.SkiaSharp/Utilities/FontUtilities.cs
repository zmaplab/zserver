using System;
using System.Collections.Concurrent;
using System.IO;
using SkiaSharp;

namespace ZMap.Renderer.SkiaSharp.Utilities
{
    public static class FontUtilities
    {
        /// <summary>
        /// Family as key
        /// </summary>
        private static readonly ConcurrentDictionary<string, SKTypeface>
            SkTypefaceCache = new();

        private static readonly SKTypeface Default = SKTypeface.FromFamilyName("宋体");

        public static void Load()
        {
            // todo: 扫描 Fonts 文件夹， 把里面的字体都缓存起来

            // 通过： cp -r myfonts /usr/share/fonts/truetype
            // chmod 777 *       不一定是必须的
            // mkfontscale
            // mkfontdir
            // fc-cache

            var folder = Path.Combine(AppContext.BaseDirectory, "fonts");
            var ttcFiles = Directory.GetFiles(folder, "*.ttc");
            foreach (var file in ttcFiles)
            {
                var typeface = SKTypeface.FromFile(file);
                SkTypefaceCache.TryAdd(typeface.FamilyName, typeface);
            }

            var ttfFiles = Directory.GetFiles(folder, "*.ttf");
            foreach (var file in ttfFiles)
            {
                var typeface = SKTypeface.FromFile(file);
                SkTypefaceCache.TryAdd(typeface.FamilyName, typeface);
            }
        }

        public static SKTypeface Get(params string[] fontFamilies)
        {
            if (fontFamilies == null || fontFamilies.Length == 0)
            {
                return Default;
            }

            foreach (var name in fontFamilies)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                return SkTypefaceCache.GetOrAdd(name, f =>
                {
                    // 1. get font from system
                    var typeface = SKFontManager.Default.MatchFamily(f);
                    if (typeface != null)
                    {
                        return typeface;
                    }

                    // 2. get font from system
                    typeface = SKTypeface.FromFamilyName(f);

                    if (typeface.FamilyName != f)
                    {
                        // 3. get font from cache, todo: weight, width, style 怎么生效？
                        typeface = Default;
                    }

                    return typeface;
                });
            }

            return Default;
        }
    }
}
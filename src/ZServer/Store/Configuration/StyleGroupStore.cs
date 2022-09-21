using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ZMap;
using ZMap.Style;
using ZMap.Utilities;
using ZServer.Extensions;

namespace ZServer.Store.Configuration
{
    public class StyleGroupStore : IStyleGroupStore
    {
        private readonly IConfiguration _configuration;
        private readonly ServerOptions _options;

        public StyleGroupStore(IConfiguration configuration, IOptionsMonitor<ServerOptions> options)
        {
            _configuration = configuration;

            _options = options.CurrentValue;
        }

        public Task<StyleGroup> FindAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var result = Cache.GetOrCreate($"{GetType().FullName}:{name}",
                entry =>
                {
                    var section =
                        _configuration.GetSection($"styleGroups:{name}");
                    StyleGroup style = null;
                    if (section.GetChildren().Any())
                    {
                        style = new StyleGroup
                        {
                            Name = name,
                            Description = section.GetOrDefault<string>("description"),
                            MinZoom = section.GetOrDefault<float>("minZoom"),
                            MaxZoom = section.GetOrDefault<float>("maxZoom"),
                            ZoomUnit = section.GetOrDefault<ZoomUnits>("zoomUnit"),
                            Styles = GetStyles(section.GetSection("styles"))
                        };
                    }

                    entry.SetValue(style);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return style;
                });

            return Task.FromResult(result);
        }

        private List<Style> GetStyles(IConfigurationSection section)
        {
            var styles = new List<Style>();
            foreach (var styleSection in section.GetChildren())
            {
                var style = styleSection.GetOrDefault<string>("type");
                var result = style switch
                {
                    "fill" => GetFillStyle(styleSection),
                    "line" => GetLineStyle(styleSection),
                    "text" => GetTextStyle(styleSection),
                    "symbol" => GetSymbolStyle(styleSection),
                    _ => null
                };

                if (result != null)
                {
                    result.Filter = styleSection.Get<string>("filter");
                    result.MinZoom = styleSection.GetOrDefault<float>("minZoom");
                    result.MaxZoom = styleSection.GetOrDefault<float>("maxZoom");
                    result.ZoomUnit = styleSection.GetOrDefault<ZoomUnits>("zoomUnit");
                    styles.Add(result);
                }
            }

            return styles;
        }

        private Style GetSymbolStyle(IConfigurationSection section)
        {
            return new SymbolStyle
            {
                Size = section.Get<int>("size"),
                Uri = section.Get<Uri>("uri")
            };
        }

        private Style GetLineStyle(IConfigurationSection section)
        {
            return new SpriteLineStyle
            {
                Opacity = section.Get<float>("opacity"),
                Width = section.Get<int>("width"),
                Color = section.Get<string>("color"),
                DashArray = section.Get<float[]>("dashArray"),
                DashOffset = section.Get<float>("dashOffset"),
                LineJoin = section.Get<string>("lineJoin"),
                Cap = section.Get<string>("cap"),
                Translate = section.Get<double[]>("translate"),
                TranslateAnchor = section.Get<TranslateAnchor>("translateAnchor"),
                GapWidth = section.Get<int>("gapWidth"),
                Offset = section.Get<int>("offset"),
                Blur = section.Get<int>("blur"),
                Gradient = section.Get<int>("gradient"),
                Pattern = section.Get<string>("pattern")
            };
        }

        private Style GetTextStyle(IConfigurationSection section)
        {
            return new TextStyle
            {
                Property = section.Get<string>("property"),
                Align = section.Get<string>("align"),
                Color = section.Get<string>("color"),
                BackgroundColor = section.Get<string>("backgroundColor"),
                Font = section.Get<string[]>("font"),
                Size = section.Get<int>("size"),
                Rotate = section.Get<float>("rotate"),
                Transform = section.Get<TextTransform>("transform"),
                Offset = section.Get<float[]>("offset")
            };
        }

        private Style GetFillStyle(IConfigurationSection section)
        {
            FillStyle symbol;
            var pattern = section.GetOrDefault<string>("pattern");
            var patternExpression = section.CreateExpression<string>("pattern");

            var resource = section.GetOrDefault<string>("resource");
            var resourceExpression = section.CreateExpression<Uri>("resource");

            if (patternExpression != null)
            {
                symbol = new SpriteFillStyle
                {
                    Pattern = patternExpression,
                    Uri = section.Get<Uri>("uri")
                };
            }
            else if (!string.IsNullOrWhiteSpace(pattern))
            {
                symbol = new SpriteFillStyle
                {
                    Pattern = Expression<string>.New(pattern),
                    Uri = section.Get<Uri>("uri")
                };
            }
            else if (resourceExpression != null)
            {
                symbol = new ResourceFillStyle
                {
                    Uri = resourceExpression
                };
            }
            else if (!string.IsNullOrWhiteSpace(resource))
            {
                if (Uri.TryCreate(resource, UriKind.Absolute, out var uri))
                {
                    symbol = new ResourceFillStyle
                    {
                        Uri = Expression<Uri>.New(uri)
                    };
                }
                else
                {
                    throw new Exception($"不是合法的资源路径: {resource}");
                }
            }
            else
            {
                symbol = new FillStyle();
            }

            symbol.Opacity = section.Get<float>("opacity");
            symbol.Antialias = section.GetValue<bool>("antialias");
            symbol.Color = section.Get<string>("color");
            symbol.Translate = section.Get<double[]>("translate");
            symbol.TranslateAnchor = section.Get<TranslateAnchor>("translateAnchor");
            return symbol;
        }

        public async Task<List<StyleGroup>> GetAllAsync()
        {
            var result = new List<StyleGroup>();
            foreach (var child in _configuration.GetSection("styleGroups").GetChildren())
            {
                result.Add(await FindAsync(child.Key));
            }

            return result;
        }
    }
}
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
        private readonly string _version;

        public StyleGroupStore(IConfiguration configuration, IOptionsMonitor<ServerOptions> options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _options = options.CurrentValue;
            _version = configuration["Version"] ?? "1.0";
        }

        public Task<StyleGroup> FindAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
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
                            Filter = section.GetExpression<bool?>("filter"),
                            Description = section.GetValue<string>("description"),
                            MinZoom = section.GetValue<float>("minZoom"),
                            MaxZoom = section.GetValue<float>("maxZoom"),
                            ZoomUnit = section.GetValue<ZoomUnits>("zoomUnit"),
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
                var style = styleSection.GetValue<string>("type");
                var result = style switch
                {
                    "fill" => GetFillStyle(styleSection),
                    "line" => GetLineStyle(styleSection),
                    "text" => GetTextStyle(styleSection),
                    "symbol" => GetSymbolStyle(styleSection),
                    _ => null
                };

                if (result == null)
                {
                    continue;
                }

                result.Filter = styleSection.GetExpression<bool?>("filter");
                result.MinZoom = styleSection.GetValue<float>("minZoom");
                result.MaxZoom = styleSection.GetValue<float>("maxZoom");
                result.ZoomUnit = styleSection.GetValue<ZoomUnits>("zoomUnit");
                styles.Add(result);
            }

            return styles;
        }

        private Style GetSymbolStyle(IConfigurationSection section)
        {
            return new SymbolStyle
            {
                Size = section.GetExpression<int>("size"),
                Uri = section.GetExpression<Uri>("uri")
            };
        }

        private Style GetLineStyle(IConfigurationSection section)
        {
            return new SpriteLineStyle
            {
                Opacity = section.GetExpression<float>("opacity"),
                Width = section.GetExpression<int>("width"),
                Color = section.GetExpression<string>("color"),
                DashArray = section.GetExpression<float[]>("dashArray"),
                DashOffset = section.GetExpression<float>("dashOffset"),
                LineJoin = section.GetExpression<string>("lineJoin"),
                LineCap = section.GetExpression<string>("lineCap"),
                Translate = section.GetExpression<double[]>("translate"),
                TranslateAnchor = section.GetExpression<TranslateAnchor>("translateAnchor"),
                GapWidth = section.GetExpression<int>("gapWidth"),
                Offset = section.GetExpression<int>("offset"),
                Blur = section.GetExpression<int>("blur"),
                Gradient = section.GetExpression<int>("gradient"),
                Pattern = section.GetExpression<string>("pattern"),
                Uri = section.GetExpression<Uri>("uri")
            };
        }

        private Style GetTextStyle(IConfigurationSection section)
        {
            var label = _version == "1.0"
                ? section.GetExpression<string>("property")
                : section.GetExpression<string>("label");
            return new TextStyle
            {
                Label = label,
                Align = section.GetExpression<string>("align"),
                Color = section.GetExpression<string>("color"),
                Opacity = section.GetExpression<float>("opacity"),
                BackgroundColor = section.GetExpression<string>("backgroundColor"),
                BackgroundOpacity = section.GetExpression<float>("backgroundOpacity"),
                Radius = section.GetExpression<float>("radius"),
                RadiusColor = section.GetExpression<string>("radiusColor"),
                RadiusOpacity = section.GetExpression<float>("radiusOpacity"),
                Style = section.GetExpression<string>("style"),
                Font = section.GetExpression<string[]>("font"),
                Size = section.GetExpression<int>("size"),
                Weight = section.GetExpression<string>("weight"),
                Rotate = section.GetExpression<float>("rotate"),
                Transform = section.GetExpression<TextTransform>("transform"),
                Offset = section.GetExpression<float[]>("offset"),
                OutlineSize = section.GetExpression<int>("outlineSize")
            };
        }

        private Style GetFillStyle(IConfigurationSection section)
        {
            FillStyle symbol;
            var pattern = section.GetValue<string>("pattern");
            var patternExpression = section.GetExpression<string>("pattern");

            var resource = section.GetValue<string>("resource");
            var resourceExpression = section.GetExpression<Uri>("resource");

            if (patternExpression != null)
            {
                symbol = new SpriteFillStyle
                {
                    Pattern = patternExpression,
                    Uri = section.GetExpression<Uri>("uri")
                };
            }
            else if (!string.IsNullOrWhiteSpace(pattern))
            {
                symbol = new SpriteFillStyle
                {
                    Pattern = Expression<string>.New(pattern),
                    Uri = section.GetExpression<Uri>("uri")
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

            symbol.Opacity = section.GetExpression<float>("opacity");
            symbol.Antialias = section.GetValue<bool>("antialias");
            symbol.Color = section.GetExpression<string>("color");
            symbol.Translate = section.GetExpression<double[]>("translate");
            symbol.TranslateAnchor = section.GetExpression<TranslateAnchor>("translateAnchor");
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZMap;
using ZMap.Style;
using ZServer.Extensions;

namespace ZServer.Store;

public class StyleGroupStore : IStyleGroupStore
{
    private static Dictionary<string, StyleGroup> _cache = new();

    public Task RefreshAsync(List<JObject> configurations)
    {
        var dict = new Dictionary<string, StyleGroup>();
        foreach (var configuration in configurations)
        {
            var sections = configuration.SelectToken("styleGroups");
            if (sections == null)
            {
                continue;
            }

            foreach (var section in sections.Children<JProperty>())
            {
                var obj = section.Value as JObject;
                if (obj == null)
                {
                    continue;
                }

                var descriptionToken = obj["description"];
                var minZoomToken = obj["minZoom"];
                var maxZoomToken = obj["maxZoom"];
                var zoomUnitValue = obj["zoomUnit"]?.ToString();
                var filter = obj.GetFilterExpressionV2();
                var zoomUnit = Enum.TryParse(zoomUnitValue, out ZoomUnits v) ? v : ZoomUnits.Scale;
                var stylesToken = obj.SelectToken("styles") as JArray;
                var styleGroup = new StyleGroup
                {
                    Name = section.Name,
                    Filter = filter,
                    Description = descriptionToken?.Value<string>(),
                    MinZoom = minZoomToken?.Value<float>() ?? 0,
                    MaxZoom = maxZoomToken?.Value<float>() ?? 20,
                    ZoomUnit = zoomUnit,
                    Styles = GetStyles(stylesToken)
                };

                dict.TryAdd(section.Name, styleGroup);
            }
        }

        _cache = dict;

        return Task.CompletedTask;
    }

    public ValueTask<StyleGroup> FindAsync(string name)
    {
        return _cache.TryGetValue(name, out var item)
            ? new ValueTask<StyleGroup>(item.Clone())
            : new ValueTask<StyleGroup>();
    }

    public ValueTask<List<StyleGroup>> GetAllAsync()
    {
        var items = _cache.Values.Select(x => x.Clone()).ToList();
        return new ValueTask<List<StyleGroup>>(items);
    }

    private List<Style> GetStyles(JArray section)
    {
        var styles = new List<Style>();
        if (section == null)
        {
            return styles;
        }

        foreach (var token in section)
        {
            if (token is not JObject styleSection)
            {
                continue;
            }

            var style = styleSection["type"]?.ToObject<string>();
            var result = style switch
            {
                "fill" => GetFillStyle(styleSection),
                "line" => GetLineStyle(styleSection),
                "spriteLine" => GetSpriteLineStyle(styleSection),
                "spriteFill" => GetSpriteFillStyle(styleSection),
                "text" => GetTextStyle(styleSection),
                "symbol" => GetSymbolStyle(styleSection),
                "Raster" => GetRasterStyle(styleSection),
                _ => null
            };

            if (result == null)
            {
                continue;
            }

            var minZoomToken = styleSection["minZoom"];
            var maxZoomToken = styleSection["maxZoom"];
            var zoomUnitValue = styleSection["zoomUnit"]?.ToString();

            result.Filter = styleSection.GetFilterExpressionV2();
            result.MinZoom = minZoomToken?.Value<float>() ?? 0;
            result.MaxZoom = maxZoomToken?.Value<float>() ?? 0;
            result.ZoomUnit = Enum.TryParse(zoomUnitValue, out ZoomUnits zoomUnit) ? zoomUnit : ZoomUnits.Scale;
            styles.Add(result);
        }

        return styles;
    }

    private Style GetSymbolStyle(JObject section)
    {
        return new SymbolStyle
        {
            Size = section.GetExpressionV2<int?>("size"),
            Uri = section.GetExpressionV2<string>("uri")
        };
    }

    private Style GetRasterStyle(JObject section)
    {
        return new RasterStyle
        {
            Opacity = section.GetExpressionV2<float?>("opacity"),
            HueRotate = section.GetExpressionV2<float?>("hueRotate"),
            BrightnessMin = section.GetExpressionV2<float?>("brightnessMin"),
            BrightnessMax = section.GetExpressionV2<float?>("brightnessMax"),
            Saturation = section.GetExpressionV2<float?>("saturation"),
            Contrast = section.GetExpressionV2<float?>("contrast")
        };
    }

    private Style GetLineStyle(JObject section)
    {
        var opacityFunc = section.GetExpressionV2<float?>("opacity");
        var widthFunc = section.GetExpressionV2<int?>("width");
        var style = new LineStyle
        {
            Opacity = opacityFunc,
            Width = widthFunc,
            Color = section.GetExpressionV2<string>("color"),
            DashArray = section.GetExpressionV2<float[]>("dashArray"),
            DashOffset = section.GetExpressionV2<float?>("dashOffset"),
            LineJoin = section.GetExpressionV2<string>("lineJoin"),
            LineCap = section.GetExpressionV2<string>("lineCap"),
            Translate = section.GetExpressionV2<double[]>("translate"),
            TranslateAnchor = section.GetExpressionV2<TranslateAnchor>("translateAnchor"),
            GapWidth = section.GetExpressionV2<int?>("gapWidth"),
            Offset = section.GetExpressionV2<int?>("offset"),
            Blur = section.GetExpressionV2<int?>("blur"),
            Gradient = section.GetExpressionV2<int?>("gradient"),
            Pattern = section.GetExpressionV2<string>("pattern")
        };
        return style;
    }

    private Style GetSpriteFillStyle(JObject section)
    {
        return new SpriteFillStyle
        {
            Opacity = section.GetExpressionV2<float?>("opacity"),
            Color = section.GetExpressionV2<string>("color"),
            Translate = section.GetExpressionV2<double[]>("translate"),
            TranslateAnchor = section.GetExpressionV2<TranslateAnchor?>("translateAnchor"),
            Pattern = section.GetExpressionV2<string>("pattern"),
            Uri = section.GetExpressionV2<string>("uri")
        };
    }

    private Style GetSpriteLineStyle(JObject section)
    {
        return new SpriteLineStyle
        {
            Opacity = section.GetExpressionV2<float?>("opacity"),
            Width = section.GetExpressionV2<int?>("width"),
            Color = section.GetExpressionV2<string>("color"),
            DashArray = section.GetExpressionV2<float[]>("dashArray"),
            DashOffset = section.GetExpressionV2<float?>("dashOffset"),
            LineJoin = section.GetExpressionV2<string>("lineJoin"),
            LineCap = section.GetExpressionV2<string>("lineCap"),
            Translate = section.GetExpressionV2<double[]>("translate"),
            TranslateAnchor = section.GetExpressionV2<TranslateAnchor>("translateAnchor"),
            GapWidth = section.GetExpressionV2<int?>("gapWidth"),
            Offset = section.GetExpressionV2<int?>("offset"),
            Blur = section.GetExpressionV2<int?>("blur"),
            Gradient = section.GetExpressionV2<int?>("gradient"),
            Pattern = section.GetExpressionV2<string>("pattern"),
            Uri = section.GetExpressionV2<string>("uri")
        };
    }

    private Style GetTextStyle(JObject section)
    {
        var label = section.GetExpressionV2<string>("property");

        if (label == null || string.IsNullOrEmpty(label.Value))
        {
            label = section.GetExpressionV2<string>("label");
        }

        var transform = section.GetExpressionV2<TextTransform>("transform");
        return new TextStyle
        {
            Label = label,
            Align = section.GetExpressionV2<string>("align"),
            Color = section.GetExpressionV2<string>("color"),
            Opacity = section.GetExpressionV2<float?>("opacity"),
            BackgroundColor = section.GetExpressionV2<string>("backgroundColor"),
            BackgroundOpacity = section.GetExpressionV2<float?>("backgroundOpacity"),
            Radius = section.GetExpressionV2<float?>("radius"),
            RadiusColor = section.GetExpressionV2<string>("radiusColor"),
            RadiusOpacity = section.GetExpressionV2<float?>("radiusOpacity"),
            Style = section.GetExpressionV2<string>("style"),
            Font = section.GetExpressionV2<List<string>>("font"),
            Size = section.GetExpressionV2<int?>("size"),
            Weight = section.GetExpressionV2<string>("weight"),
            Rotate = section.GetExpressionV2<float?>("rotate"),
            Transform = transform,
            Offset = section.GetExpressionV2<float[]>("offset"),
            OutlineSize = section.GetExpressionV2<int?>("outlineSize")
        };
    }

    private Style GetFillStyle(JObject section)
    {
        FillStyle fillStyle;
        var pattern = section["pattern"]?.ToObject<string>();
        var patternExpression = section.GetExpressionV2<string>("pattern");

        var resource = section["resource"]?.ToObject<string>();
        var resourceExpression = section.GetExpressionV2<string>("resource");

        if (patternExpression != null && (
                // !string.IsNullOrEmpty(patternExpression.Script) ||
                !string.IsNullOrEmpty(patternExpression.Value)))
        {
            fillStyle = new SpriteFillStyle
            {
                Pattern = patternExpression,
                Uri = section.GetExpressionV2<string>("uri")
            };
        }
        else if (!string.IsNullOrEmpty(pattern))
        {
            fillStyle = new SpriteFillStyle
            {
                Pattern = CSharpExpressionV2.Create<string>(pattern),
                Uri = section.GetExpressionV2<string>("uri")
            };
        }
        else if (resourceExpression != null && !string.IsNullOrEmpty(resourceExpression.Value))
        {
            fillStyle = new ResourceFillStyle
            {
                Uri = resourceExpression
            };
        }
        else if (!string.IsNullOrEmpty(resource))
        {
            fillStyle = new ResourceFillStyle
            {
                Uri = CSharpExpressionV2.Create<string>(resource)
            };
        }
        else
        {
            fillStyle = new FillStyle();
        }

        var o = section.GetExpressionV2<float?>("opacity");
        fillStyle.Opacity = o;
        fillStyle.Pattern = section.GetExpressionV2<string>("pattern");
        fillStyle.Antialias = section["antialias"]?.ToObject<bool>() ?? true;
        fillStyle.Color = section.GetExpressionV2<string>("color");
        // symbol.OutlineColor = section.GetExpression<string>("outlineColor");
        fillStyle.Translate = section.GetExpressionV2<double[]>("translate");
        fillStyle.TranslateAnchor = section.GetExpressionV2<TranslateAnchor?>("translateAnchor");
        return fillStyle;
    }
}
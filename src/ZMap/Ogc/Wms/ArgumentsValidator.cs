using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ZMap.Ogc.Wms;

public static class ArgumentsValidator
{
    public static (string Code, string Message, RequestArguments Arguments) VerifyAndBuildWmsGetMapArguments(
        string layers,
        string srs, string bbox,
        int width, int height, string format)
    {
        if (string.IsNullOrWhiteSpace(layers))
        {
            return ("LayerNotDefined", "No layers have been requested", null);
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return ("InvalidFormat", "No output map format requested", null);
        }

        if (string.IsNullOrWhiteSpace(srs))
        {
            return ("InvalidSRS", "No srs requested", null);
        }

        if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
        {
            return ("InvalidSRS", "SRS is not valid", null);
        }

        if (string.IsNullOrWhiteSpace(bbox))
        {
            return ("MissingBBox", "GetMap requests must include a BBOX parameter", null);
        }

        var points = bbox.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
            !float.TryParse(points[1], out var y1)
            || !float.TryParse(points[2], out var x2)
            || !float.TryParse(points[3], out var y2))
        {
            return ("InvalidBBox", $"The request bounding box is invalid: {bbox}", null);
        }

        if (x2 - x1 <= 0 || y2 - y1 <= 0)
        {
            return ("InvalidBBox", $"The request bounding box has zero area: {bbox}", null);
        }

        if (width <= 0 || height <= 0)
        {
            return (
                "MissingOrInvalidParameter",
                $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}",
                null);
        }

        var list = new List<(string Group, string Layer)>();
        foreach (var layer in layers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var layerQuery = layer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            switch (layerQuery.Length)
            {
                case 2:
                    list.Add((layerQuery[0], layerQuery[1]));
                    break;
                case 1:
                    list.Add((null, layerQuery[0]));
                    break;
                default:
                {
                    // todo: 重新描述错误
                    return ("LayerNotDefined", $"can not find layer {layer}", null);
                }
            }
        }

        if (list.Count == 0)
        {
            return ("LayerNotDefined", $"can not find layer {layers}", null);
        }

        var envelope = new Envelope(x1, x2, y1, y2);
        return (null, null, new RequestArguments
        {
            Envelope = envelope, SRID = srid, Layers = list
        });
    }

    public static (string Code, string Message, RequestArguments Arguments) VerifyAndBuildWmsGetFeatureInfoArguments(
        string layers, string srs, string bbox,
        int width, int height, double x, double y, int featureCount)
    {
        if (string.IsNullOrWhiteSpace(layers))
        {
            return ("LayerNotDefined", "No layers have been requested", null);
        }

        if (string.IsNullOrWhiteSpace(srs))
        {
            return ("InvalidSRS", "No srs requested", null);
        }

        if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
        {
            return ("InvalidSRS", "SRS is not valid", null);
        }

        if (string.IsNullOrWhiteSpace(bbox))
        {
            return ("MissingBBox", "GetFeatureInfo requests must include a BBOX parameter", null);
        }

        var points = bbox.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
            !float.TryParse(points[1], out var y1)
            || !float.TryParse(points[2], out var x2)
            || !float.TryParse(points[3], out var y2))
        {
            return ("InvalidBBox", $"The request bounding box is invalid: {bbox}", null);
        }

        if (x2 - x1 <= 0 || y2 - y1 <= 0)
        {
            return ("InvalidBBox", $"The request bounding box has zero area: {bbox}", null);
        }

        if (width <= 0 || height <= 0)
        {
            return ("MissingOrInvalidParameter",
                $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}",
                null);
        }

        // if (x < 0 || y < 0 || x > width || y > height)
        // {
        //     return new ModeState($"x, y is invalid", "InvalidXY");
        // }

        if (featureCount <= 0)
        {
            return ("InvalidFeatureCount", $"featureCount is invalid", null);
        }

        var list = new List<(string Group, string Layer)>();
        foreach (var layer in layers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var layerQuery = layer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            switch (layerQuery.Length)
            {
                case 2:
                    list.Add((layerQuery[0], layerQuery[1]));

                    break;
                case 1:
                    list.Add((null, layerQuery[0]));
                    break;
                default:
                {
                    // todo: 重新描述错误
                    return ("LayerNotDefined", $"Could not find layer {layer}", null);
                }
            }
        }

        if (list.Count == 0)
        {
            return ("LayerNotDefined", $"Could not find layer {layers}", null);
        }

        var envelope = new Envelope(x1, x2, y1, y2);
        return (null, null, new RequestArguments
        {
            Envelope = envelope, SRID = srid, Layers = list
        });
    }
}
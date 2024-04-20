using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ZMap.Ogc.Wms;

public static class ArgumentsValidator
{
    public static ValidateResult VerifyAndBuildWmsGetMapArguments(
        string layers,
        string srs, string bbox,
        int width, int height, string format)
    {
        if (string.IsNullOrWhiteSpace(layers))
        {
            return new ValidateResult(null, "LayerNotDefined", "No layers have been requested");
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return new ValidateResult(null, "InvalidFormat", "No output map format requested");
        }

        if (string.IsNullOrWhiteSpace(srs))
        {
            return new ValidateResult(null, "InvalidSRS", "No srs requested");
        }

        if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
        {
            return new ValidateResult(null, "InvalidSRS", "SRS is not valid");
        }

        if (string.IsNullOrWhiteSpace(bbox))
        {
            return new ValidateResult(null, "MissingBBox", "GetMap requests must include a BBOX parameter");
        }

        var points = bbox.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
            !float.TryParse(points[1], out var y1)
            || !float.TryParse(points[2], out var x2)
            || !float.TryParse(points[3], out var y2))
        {
            return new ValidateResult(null, "InvalidBBox", $"The request bounding box is invalid: {bbox}");
        }

        if (x2 - x1 <= 0 || y2 - y1 <= 0)
        {
            return new ValidateResult(null, "InvalidBBox", $"The request bounding box has zero area: {bbox}");
        }

        if (width <= 0 || height <= 0)
        {
            return new ValidateResult(
                null, "MissingOrInvalidParameter",
                $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}");
        }

        var list = new List<(string Group, string Layer)>();
        foreach (var layer in layers.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var layerQuery = layer.Split(':', StringSplitOptions.RemoveEmptyEntries);

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
                    return new ValidateResult(null, "LayerNotDefined", $"can not find layer {layer}");
                }
            }
        }

        if (list.Count == 0)
        {
            return new ValidateResult(null, "LayerNotDefined", $"can not find layer {layers}");
        }

        var envelope = new Envelope(x1, x2, y1, y2);
        return new ValidateResult(new RequestArguments(envelope, srid, list), null, null);
    }

    public static ValidateResult VerifyAndBuildWmsGetFeatureInfoArguments(
        string layers, string srs, string bbox,
        int width, int height, double x, double y, int featureCount)
    {
        if (string.IsNullOrWhiteSpace(layers))
        {
            return new ValidateResult(null, "LayerNotDefined", "No layers have been requested");
        }

        if (string.IsNullOrWhiteSpace(srs))
        {
            return new ValidateResult(null, "InvalidSRS", "No srs requested");
        }

        if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
        {
            return new ValidateResult(null, "InvalidSRS", "SRS is not valid");
        }

        if (string.IsNullOrWhiteSpace(bbox))
        {
            return new ValidateResult(null, "MissingBBox", "GetFeatureInfo requests must include a BBOX parameter");
        }

        var points = bbox.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
            !float.TryParse(points[1], out var y1)
            || !float.TryParse(points[2], out var x2)
            || !float.TryParse(points[3], out var y2))
        {
            return new ValidateResult(null, "InvalidBBox", $"The request bounding box is invalid: {bbox}");
        }

        if (x2 - x1 <= 0 || y2 - y1 <= 0)
        {
            return new ValidateResult(null, "InvalidBBox", $"The request bounding box has zero area: {bbox}");
        }

        if (width <= 0 || height <= 0)
        {
            return new ValidateResult(null, "MissingOrInvalidParameter",
                $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}");
        }

        // if (x < 0 || y < 0 || x > width || y > height)
        // {
        //     return new ModeState($"x, y is invalid", "InvalidXY");
        // }

        if (featureCount <= 0)
        {
            return new ValidateResult(null, "InvalidFeatureCount", $"featureCount is invalid");
        }

        var list = new List<(string Group, string Layer)>();
        foreach (var layer in layers.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var layerQuery = layer.Split(':', StringSplitOptions.RemoveEmptyEntries);

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
                    return new ValidateResult(null, "LayerNotDefined", $"Could not find layer {layer}");
                }
            }
        }

        if (list.Count == 0)
        {
            return new ValidateResult(null, "LayerNotDefined", $"Could not find layer {layers}");
        }

        var envelope = new Envelope(x1, x2, y1, y2);
        return new ValidateResult(new RequestArguments(envelope, srid, list), null, null);
    }
}
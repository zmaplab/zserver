// using System;
// using System.Collections.Generic;
//
// namespace ZServer.Interfaces;
//
// public static class ModeStateUtilities
// {
//     public static ModeState VerifyWmsGetMapArguments(string layers, string srs, string bbox,
//         int width, int height, string format)
//     {
//         if (string.IsNullOrWhiteSpace(layers))
//         {
//             return new ModeState("No layers have been requested", "LayerNotDefined");
//         }
//
//         if (string.IsNullOrWhiteSpace(format))
//         {
//             return new ModeState("No output map format requested", "InvalidFormat");
//         }
//
//         if (string.IsNullOrWhiteSpace(srs))
//         {
//             return new ModeState("No srs requested", "InvalidSRS");
//         }
//
//         if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
//         {
//             return new ModeState("SRS is not valid", "InvalidSRS");
//         }
//
//         if (string.IsNullOrWhiteSpace(bbox))
//         {
//             return new ModeState("GetMap requests must include a BBOX parameter", "MissingBBox");
//         }
//
//         var points = bbox.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//         if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
//             !float.TryParse(points[1], out var y1)
//             || !float.TryParse(points[2], out var x2)
//             || !float.TryParse(points[3], out var y2))
//         {
//             return new ModeState($"The request bounding box is invalid: {bbox}", "InvalidBBox");
//         }
//
//         if (x2 - x1 <= 0 || y2 - y1 <= 0)
//         {
//             return new ModeState($"The request bounding box has zero area: {bbox}", "InvalidBBox");
//         }
//
//         if (width <= 0 || height <= 0)
//         {
//             return new ModeState(
//                 $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}",
//                 "MissingOrInvalidParameter");
//         }
//
//         var list = new List<(string Group, string Layer)>();
//         foreach (var layer in layers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
//         {
//             var layerQuery = layer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
//
//             switch (layerQuery.Length)
//             {
//                 case 2:
//                     list.Add((layerQuery[0], layerQuery[1]));
//                     break;
//                 case 1:
//                     list.Add((null, layerQuery[0]));
//                     break;
//                 default:
//                 {
//                     // todo: 重新描述错误
//                     return new ModeState(
//                         $"can not find layer {layer}",
//                         "LayerNotDefined");
//                 }
//             }
//         }
//
//         if (list.Count == 0)
//         {
//             return new ModeState(
//                 $"can not find layer {layers}",
//                 "LayerNotDefined");
//         }
//
//         return new ModeState
//         {
//             MinX = x1, MaxX = x2, MinY = y1, MaxY = y2, SRID = srid, Layers = list
//         };
//     }
//
//     public static ModeState VerifyWmsGetFeatureInfoArguments(string layers, string srs, string bbox,
//         int width, int height, double x, double y, int featureCount)
//     {
//         if (string.IsNullOrWhiteSpace(layers))
//         {
//             return new ModeState("No layers have been requested", "LayerNotDefined");
//         }
//
//         if (string.IsNullOrWhiteSpace(srs))
//         {
//             return new ModeState("No srs requested", "InvalidSRS");
//         }
//
//         if (!int.TryParse(srs.Replace("EPSG:", ""), out var srid))
//         {
//             return new ModeState("SRS is not valid", "InvalidSRS");
//         }
//
//         if (string.IsNullOrWhiteSpace(bbox))
//         {
//             return new ModeState("GetFeatureInfo requests must include a BBOX parameter", "MissingBBox");
//         }
//
//         var points = bbox.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//         if (points.Length != 4 || !float.TryParse(points[0], out var x1) ||
//             !float.TryParse(points[1], out var y1)
//             || !float.TryParse(points[2], out var x2)
//             || !float.TryParse(points[3], out var y2))
//         {
//             return new ModeState($"The request bounding box is invalid: {bbox}", "InvalidBBox");
//         }
//
//         if (x2 - x1 <= 0 || y2 - y1 <= 0)
//         {
//             return new ModeState($"The request bounding box has zero area: {bbox}", "InvalidBBox");
//         }
//
//         if (width <= 0 || height <= 0)
//         {
//             return new ModeState(
//                 $"Missing or invalid requested map size. Parameters WIDTH and HEIGHT shall be present and be integers > 0. Got WIDTH={width}, HEIGHT={height}",
//                 "MissingOrInvalidParameter");
//         }
//
//         // if (x < 0 || y < 0 || x > width || y > height)
//         // {
//         //     return new ModeState($"x, y is invalid", "InvalidXY");
//         // }
//
//         if (featureCount <= 0)
//         {
//             return new ModeState($"featureCount is invalid", "InvalidFeatureCount");
//         }
//
//         var list = new List<(string Group, string Layer)>();
//         foreach (var layer in layers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
//         {
//             var layerQuery = layer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
//
//             switch (layerQuery.Length)
//             {
//                 case 2:
//                     list.Add((layerQuery[0], layerQuery[1]));
//
//                     break;
//                 case 1:
//                     list.Add((null, layerQuery[0]));
//                     break;
//                 default:
//                 {
//                     // todo: 重新描述错误
//                     return new ModeState(
//                         $"Could not find layer {layer}",
//                         "LayerNotDefined");
//                 }
//             }
//         }
//
//         if (list.Count == 0)
//         {
//             return new ModeState(
//                 $"Could not find layer {layers}",
//                 "LayerNotDefined");
//         }
//
//         return new ModeState
//         {
//             MinX = x1, MaxX = x2, MinY = y1, MaxY = y2, SRID = srid, Layers = list
//         };
//     }
// }
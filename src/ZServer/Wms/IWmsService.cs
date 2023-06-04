using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NetTopologySuite.Features;

namespace ZServer.Wms;

public interface IWmsService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bgColor"></param>
    /// <param name="time"></param>
    /// <param name="layers">LAYERS</param>
    /// <param name="format">FORMAT</param>
    /// <param name="srs">SRS</param>
    /// <param name="bbox">BBOX</param>
    /// <param name="width">WIDTH</param>
    /// <param name="height">HEIGHT</param>
    /// <param name="cqlFilter">CQL_FILTER</param>
    /// <param name="styles"></param>
    /// <param name="formatOptions">FORMAT_OPTIONS</param>
    /// <param name="extras"></param>
    /// <param name="transparent"></param>
    /// <returns></returns>
    Task<(string Code, string Message, Stream Stream)> GetMapAsync(string layers, string styles, string srs,
        string bbox,
        int width, int height, string format, bool transparent, string bgColor, int time,
        string formatOptions, string cqlFilter, IDictionary<string, object> extras);

    Task<(string Code, string Message, FeatureCollection Features)> GetFeatureInfoAsync(string layers, string infoFormat, int featureCount,
        string srs,
        string bbox, int width, int height,
        double x, double y, IDictionary<string, object> arguments);
}
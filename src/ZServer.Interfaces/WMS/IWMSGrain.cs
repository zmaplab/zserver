using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using Orleans;

namespace ZServer.Interfaces.WMS
{
    // ReSharper disable once InconsistentNaming
    public interface IWMSGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgcolor"></param>
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
        /// <param name="arguments"></param>
        /// <param name="transparent"></param>
        /// <returns></returns>
        ValueTask<MapResult> GetMapAsync(string layers, string styles, string srs, string bbox,
            int width, int height, string format, bool transparent, string bgcolor, int time,
            string formatOptions, string cqlFilter, IDictionary<string, object> arguments);

        ValueTask<MapResult> GetCapabilitiesAsync(string version, string format, IDictionary<string, object> arguments);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layers">查询图层</param>
        /// <param name="infoFormat">返回格式， 默认 JSON</param>
        /// <param name="featureCount">最大返回个数</param>
        /// <param name="height"></param>
        /// <param name="x">当前返回图像水平方向的像素值， 左上角为原点(0,0)</param>
        /// <param name="y">当前返回图像垂直方向的像素值， (I,J)为指定像素中心</param>
        /// <param name="srs"></param>
        /// <param name="bbox"></param>
        /// <param name="width"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        ValueTask<FeatureCollection> GetFeatureInfoAsync(string layers, string infoFormat, int featureCount, string srs,
            string bbox,
            int width, int height, double x, double y, IDictionary<string, object> arguments);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace ZServer.Interfaces.WMS;

// ReSharper disable once InconsistentNaming
public interface IWMSGrain : IGrainWithIntegerKey
{
    ValueTask<ZServerResponse> GetCapabilitiesAsync(string version, string format,
        IDictionary<string, object> arguments);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layers">地图图层列表，地图图层之间以半角英文逗号进行分隔。
    /// 最左边的图层在最底，下一个图层放到前一个的上面，依次类推。
    /// 图层名称必须是 GetCapabilities 操作返回的文档中声明的 Name 元素的值。</param>
    /// <param name="styles">请求图层的样式列表，图层样式之间以逗号分隔。
    /// styles 值与 layers 参数值必须是一一对应的。如果请求不存在的 Style ，服务器将返回一个 Code 为 StyleNotDefined 的异常。</param>
    /// <param name="srs">空间坐标参考系统。该参数值应该是 GetCapabilities 操作中服务器声明的 SRS。</param>
    /// <param name="bbox">Bounding box（地图范围），该参数的值为半角英文逗号分隔的一串实数，形如“minx,miny,maxx,maxy”，分别代表指定 SRS 下的区域坐标最小 X、最小 Y、最大 X、最大 Y。</param>
    /// <param name="width">地图图片的像素宽度。</param>
    /// <param name="height">地图图片的像素高度。</param>
    /// <param name="format">地图的输出格式。</param>
    /// <param name="transparent">地图的背景是否透明（默认为 false）。</param>
    /// <param name="bgColor">背景色的十六进制红绿蓝颜色值（默认为 0xFFFFFF）。</param>
    /// <param name="time">希望获取图层的时间值。不考虑实现。</param>
    /// <param name="cqlFilter">CQL_FILTER</param>
    /// <param name="formatOptions">FORMAT_OPTIONS</param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    ValueTask<ZServerResponse> GetMapAsync(string layers, string styles, string srs, string bbox,
        int width, int height, string format, bool transparent, string bgColor, int time,
        string formatOptions, string cqlFilter, IDictionary<string, object> arguments);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layers">查询图层</param>
    /// <param name="srs"></param>
    /// <param name="bbox"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="infoFormat">返回格式， 默认 JSON</param>
    /// <param name="featureCount">最大返回个数</param>
    /// <param name="x">当前返回图像水平方向的像素值， 左上角为原点(0,0)</param>
    /// <param name="y">当前返回图像垂直方向的像素值， (I,J)为指定像素中心</param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    ValueTask<ZServerResponse> GetFeatureInfoAsync(string layers, string srs, string bbox, int width, int height,
        string infoFormat, int featureCount,
        double x, double y, IDictionary<string, object> arguments);
}
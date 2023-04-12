using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace ZServer.Interfaces.WMTS
{
    /// <summary>
    /// http://t0.tianditu.com/img_w/wmts
    /// ?service=wmts&request=GetTile&version=1.0.0&LAYER=img&tileMatrixSet=w&TileMatrix={TileMatrix}&TileRow={TileRow}&TileCol={TileCol}&style=default&format=tiles
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IWMTSGrain : IGrainWithStringKey
    {
        Task<MapResult> GetTileAsync(string layers, string styles, string format,
            string tileMatrixSet, string tileMatrix, int tileRow, int tileCol, IDictionary<string, object> arguments,
            string cqlFilter = default);
    }
}
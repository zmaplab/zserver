using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZServer.Wmts;

public interface IWmtsService
{
    Task<(string Code, string Message, Stream Stream)> GetTileAsync(string layers, string styles, string format,
        string tileMatrixSet,
        string tileMatrix, int tileRow,
        int tileCol, string cqlFilter, IDictionary<string, object> arguments);
}
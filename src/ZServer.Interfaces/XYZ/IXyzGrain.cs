using System.Threading.Tasks;
using Orleans;

namespace ZServer.Interfaces.XYZ;

public interface IXyzGrain: IGrainWithStringKey
{
    Task<ZServerResponse> GetMapAsync(string layers, int x, int y, int z);
}
using System.Threading.Tasks;
using Orleans;

namespace ZServer.Interfaces.XYZ
{
    public interface IXyzGrain: IGrainWithStringKey
    {
        Task<MapResult> GetMapAsync(string layers, int x, int y, int z);
    }
}
using System.Threading.Tasks;
using Orleans;
using ZServer.Interfaces;
using ZServer.Interfaces.XYZ;

namespace ZServer.Grains.XYZ
{
    public class XyzGrain : Grain, IXyzGrain
    {
        public Task<MapResult> GetMapAsync(string layers, int x, int y, int z)
        {
            throw new System.NotImplementedException();
        }
    }
}
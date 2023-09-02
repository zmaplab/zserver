using System.Threading.Tasks;
using ZMap.TileGrid;

namespace ZMap.Store
{
    public interface IGridSetStore : IRefresher
    {
        Task<GridSet> FindAsync(string name);
    }
}
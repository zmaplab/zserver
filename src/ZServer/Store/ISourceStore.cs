using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Source;

namespace ZServer.Store
{
    public interface ISourceStore
    {
        Task<ISource> FindAsync(string name);
        Task<List<ISource>> GetAllAsync();
    }
}
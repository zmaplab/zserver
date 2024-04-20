using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Source;
using ZMap.Store;

namespace ZServer.Store;

public interface ISourceStore : IRefresher
{
    Task<ISource> FindAsync(string name);
    Task<List<ISource>> GetAllAsync();
}
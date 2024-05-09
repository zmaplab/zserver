using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Source;
using ZMap.Store;

namespace ZServer.Store;

public interface ISourceStore : IRefresher
{
    ValueTask<ISource> FindAsync(string name);
    ValueTask<List<ISource>> GetAllAsync();
}
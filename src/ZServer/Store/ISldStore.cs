using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Store;
using ZMap.Style;

namespace ZServer.Store;

public interface ISldStore : IRefresher
{
    Task<List<StyleGroup>> FindAsync(string name);
}
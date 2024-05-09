using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Store;
using ZMap.Style;


namespace ZServer.Store;

public interface IStyleGroupStore : IRefresher
{
    ValueTask<StyleGroup> FindAsync(string name);
    ValueTask<List<StyleGroup>> GetAllAsync();
}
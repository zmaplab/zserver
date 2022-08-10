using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Style;


namespace ZServer.Store
{
    public interface IStyleGroupStore
    {
        Task<StyleGroup> FindAsync(string name);
        Task<List<StyleGroup>> GetAllAsync();
    }
}
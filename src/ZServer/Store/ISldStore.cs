using System.Collections.Generic;
using System.Threading.Tasks;
using ZMap.Style;

namespace ZServer.Store;

public interface ISldStore
{
    Task<List<StyleGroup>> FindAsync(string name);
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ZServer.Store;

public interface IJsonStoreProvider
{
    Task<List<JObject>> GetConfigurationAsync();
    void Check();
}
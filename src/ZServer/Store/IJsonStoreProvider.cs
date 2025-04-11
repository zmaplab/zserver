using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ZServer.Store;

public interface IJsonStoreProvider
{
    Task<JObject> GetConfigurationAsync();
    void Check();
}
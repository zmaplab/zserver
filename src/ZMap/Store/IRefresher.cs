

namespace ZMap.Store;

public interface IRefresher
{
    Task Refresh(List<JObject> configurations);
}
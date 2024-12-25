namespace ZMap.Store;

/// <summary>
/// 刷新器
/// </summary>
public interface IRefresher
{
    Task RefreshAsync(List<JObject> configurations);
}
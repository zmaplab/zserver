namespace ZServer.API.Controllers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Msg { get; set; }
}
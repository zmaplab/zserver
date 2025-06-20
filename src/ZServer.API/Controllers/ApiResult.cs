namespace ZServer.API.Controllers;

public class ApiResult<T>
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Msg { get; set; }
}
namespace ZMap.Ogc;

public record MapResult(Stream Stream, string Code, string Message, string Locator = null) : IDisposable
{
    public bool IsSuccess()
    {
        return Code is null or "" or "0" or "200";
    }

    public void Dispose()
    {
        Stream?.Dispose();
    }
}
namespace ZServer.Interfaces;

public static class ZServerResponseFactory
{
    public static ZServerResponse Failed(string message, string code, string locator = null)
    {
        return new ZServerResponse
        {
            Exception = new ServerException
            {
                Text = message,
                Code = code,
                Locator = locator
            },
            Body = null,
            ContentType = null
        };
    }

    public static ZServerResponse Ok(byte[] bytes, string contentType)
    {
        return new ZServerResponse
        {
            Exception = null,
            ContentType = contentType,
            Body = bytes
        };
    }
}
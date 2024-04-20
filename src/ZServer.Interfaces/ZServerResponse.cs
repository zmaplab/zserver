using Orleans;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ZServer.Interfaces;

[GenerateSerializer]
[Immutable]
public record ZServerResponse
{
    /// <summary>
    /// 异常为空则成功
    /// </summary>
    [Id(0)]
    public ServerException Exception { get; init; }

    /// <summary>
    /// 内容格式
    /// </summary>
    [Id(1)]
    public string ContentType { get; init; }

    /// <summary>
    /// TODO: 返回有 String/Image 两种
    /// </summary>
    [Id(2)]
    public byte[] Body { get; init; }
}
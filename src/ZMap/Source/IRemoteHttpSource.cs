using System.Net.Http;

namespace ZMap.Source;

public interface IRemoteHttpSource : ISource
{
    IHttpClientFactory HttpClientFactory { get; set; }
}
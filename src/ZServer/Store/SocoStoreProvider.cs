using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;

namespace ZServer.Store;

public class SocoStoreProvider(string url, IHttpClientFactory factory, ILogger<SocoStoreProvider> logger)
    : IJsonStoreProvider
{
    private string _lastHash;
    private static readonly string AppId;
    private static readonly string AppSecret;

    private static readonly List<string> ExcludeParams =
        ["appId", "nonce", "timestamp", "sign"];

    static SocoStoreProvider()
    {
        AppId = Environment.GetEnvironmentVariable("ZSERVER_SOCODB_APPID");
        AppSecret = Environment.GetEnvironmentVariable("ZSERVER_SOCODB_APPSECRET");
    }

    public async Task<JObject> GetConfigurationAsync()
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        var nonce = ObjectId.GenerateNewId().ToString();
        requestMessage.Headers.TryAddWithoutValidation("AppId", AppId);
        requestMessage.Headers.TryAddWithoutValidation("Nonce", nonce);
        var ts = DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds().ToString();
        requestMessage.Headers.TryAddWithoutValidation("Timestamp", ts);
        var absolutePath = new Uri(url).AbsolutePath;
        var index = absolutePath.IndexOf("/v1.", StringComparison.Ordinal);
        var path = absolutePath.Substring(index);
        var signData = GetSignData(AppId, path, null, null, nonce, ts);
        var sign = Sign(AppSecret, signData);
        requestMessage.Headers.TryAddWithoutValidation("Sign", sign);

        var response = await factory.CreateClient("HttpJsonStoreProvider").SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            logger.LogError("Read zserver config error: {Message}, {Status}", result, response.StatusCode);
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var hash = CryptographyUtility.ComputeHash(bytes);
        if (hash == _lastHash)
        {
            return null;
        }

        _lastHash = hash;
        var json = Encoding.UTF8.GetString(bytes).Replace("\uFEFF", "").Replace("\u200B", "");

        return Read(json);
    }

    public void Check()
    {
    }

    private static string Sign(string key, byte[] expectedSignData)
    {
        using var algorithm = new HMACSHA1();
        algorithm.Key = Convert.FromBase64String(key);
        var signature = Convert.ToBase64String(
            algorithm.ComputeHash(expectedSignData));
        return signature;
    }

    private static byte[] GetSignData(string appId, string path, IEnumerable<KeyValuePair<string, StringValues>> query,
        string body, string nonce, string timestamp)
    {
        var data = new StringBuilder();
        data.AppendLine(appId);

        data.AppendLine(nonce);
        data.AppendLine(timestamp);
        data.AppendLine(path);
        if (query != null)
        {
            foreach (var kv in query)
            {
                var shouldExclude =
                    ExcludeParams.Any(param => param.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));

                if (shouldExclude)
                {
                    continue;
                }

                data.Append('&').Append(kv.Key).Append('=').Append(kv.Value);
            }
        }

        data.AppendLine(body ?? string.Empty);
        var str = data.ToString();
        return Encoding.UTF8.GetBytes(str);
    }

    public static JObject Read(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        if (JsonConvert.DeserializeObject(json) is not JObject resultObject)
        {
            return null;
        }

        if (resultObject["data"] is JArray array)
        {
            if (array.Count == 0)
            {
                return null;
            }

            if (array[0] is not JObject first)
            {
                return null;
            }

            var config = first["config"]?.ToString();
            if (string.IsNullOrEmpty(config))
            {
                return null;
            }

            return JsonConvert.DeserializeObject(config) as JObject;
        }

        if (resultObject["data"] is not JObject jObject)
        {
            return null;
        }

        var config1 = jObject["config"]?.ToString();
        if (string.IsNullOrEmpty(config1))
        {
            return null;
        }

        return JsonConvert.DeserializeObject(config1) as JObject;
    }
}
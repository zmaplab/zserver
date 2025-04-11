using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;

namespace ZServer.Store;

public class FileJsonStoreProvider(string path, ILoggerFactory loggerFactory) : IJsonStoreProvider
{
    private DateTime _lastWriteTime;
    private string _lastHash;

    public string Path => path;

    public async Task<JObject> GetConfigurationAsync()
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var file = new FileInfo(path);
        if (file.LastWriteTime == _lastWriteTime)
        {
            return null;
        }

        _lastWriteTime = file.LastWriteTime;

        var bytes = await File.ReadAllBytesAsync(path);
        var hash = CryptographyUtility.ComputeHash(bytes);
        if (hash == _lastHash)
        {
            return null;
        }

        _lastHash = hash;
        var json = Encoding.UTF8.GetString(bytes).Replace("\uFEFF", "").Replace("\u200B", "");

        var result = JsonConvert.DeserializeObject(json) as JObject;
        await Task.CompletedTask;
        return result;
    }

    public void Check()
    {
        var logger = loggerFactory.CreateLogger("FileJsonStoreProvider");
        if (File.Exists(Path))
        {
            logger.LogInformation("ZServer 发现配置文件 {ConfigurationPath} ",
                Path);
        }
        else
        {
            logger.LogError("ZServer 未发现配置文件 {ConfigurationPath}", Path);
        }
    }
}
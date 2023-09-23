using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;

namespace ZServer.Store;

public class ConfigurationProvider
{
    private readonly string _path;
    private DateTime _lastWriteTime;
    private string _lastHash;

    public string Path => _path;

    public ConfigurationProvider(string path)
    {
        _path = path;
    }

    public JObject GetConfiguration()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        var file = new FileInfo(_path);
        if (file.LastWriteTime == _lastWriteTime)
        {
            return null;
        }

        _lastWriteTime = file.LastWriteTime;

        var bytes = File.ReadAllBytes(_path);
        var hash = MurmurHashAlgorithmUtilities.ComputeHash(bytes);
        if (hash == _lastHash)
        {
            return null;
        }

        _lastHash = hash;
        var json = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject(json) as JObject;
    }
}
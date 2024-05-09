using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMap.Infrastructure;

namespace ZServer.Store;

public class JsonStoreProvider(string path)
{
    private DateTime _lastWriteTime;
    private string _lastHash;

    public string Path => path;

    public JObject GetConfiguration()
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

        var bytes = File.ReadAllBytes(path);
        var hash = MurmurHashAlgorithmUtility.ComputeHash(bytes);
        if (hash == _lastHash)
        {
            return null;
        }

        _lastHash = hash;
        var json = Encoding.UTF8.GetString(bytes).Replace("\uFEFF", "").Replace("\u200B", "");
        ;
        return JsonConvert.DeserializeObject(json) as JObject;
    }
}
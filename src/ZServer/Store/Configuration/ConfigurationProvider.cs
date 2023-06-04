using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using ZMap.Infrastructure;

namespace ZServer.Store.Configuration;

public class ConfigurationProvider
{
    private readonly string _path;
    private DateTime _lastWriteTime;
    private string _lastHash;

    public ConfigurationProvider(string path)
    {
        _path = path;
    }

    public IConfiguration GetConfiguration()
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

        var json = File.ReadAllBytes(_path);
        var hash = MurmurHashAlgorithmService.ComputeHash(json);
        if (hash == _lastHash)
        {
            return null;
        }

        _lastHash = hash;
        var configurationBuilder = new ConfigurationBuilder();
        using var stream = File.OpenRead(_path);
        configurationBuilder.AddJsonStream(stream);
        var configuration= configurationBuilder.Build();
        return configuration;
    }
}
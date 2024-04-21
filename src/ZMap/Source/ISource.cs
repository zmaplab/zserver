using System;
using NetTopologySuite.Geometries;

namespace ZMap.Source;

/// <summary>
/// TODO: 做成无状态类
/// </summary>
public interface ISource : IDisposable
{
    string Name { get; set; }
    int Srid { get; }
    Envelope GetEnvelope();
    ISource Clone();
}
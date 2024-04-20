using System;
using NetTopologySuite.Geometries;

namespace ZMap.Source;

public interface ISource : IDisposable
{
    string Name { get; set; }
    int Srid { get; }
    Envelope GetEnvelope();
    ISource Clone();
}
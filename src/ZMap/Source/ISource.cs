using System;
using NetTopologySuite.Geometries;

namespace ZMap.Source
{
    public interface ISource : IDisposable
    {
        int SRID { get; }
        Envelope GetEnvelope();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace ZMap.Source;

public abstract class VectorSourceBase : IVectorSource
{
    public string Name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public int Srid { get; set; }

    public string Filter { get; set; }

    public abstract Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent);

    public abstract Envelope GetEnvelope();
    public abstract ISource Clone();
    public abstract void Dispose();
}
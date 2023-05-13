using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace ZMap.Source
{
    public abstract class VectorSourceBase : IVectorSource
    {
        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int SRID { get; set; }

        public IFeatureFilter Filter { get; set; }

        public abstract Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent);

        public abstract Envelope GetEnvelope();

        public abstract void Dispose();
    }
}
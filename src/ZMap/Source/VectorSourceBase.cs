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

        public IFeatureFilter Filter { get; private set; }

        public void SetFilter(IFeatureFilter filter)
        {
            Filter = filter;
        }

        public abstract ValueTask<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent);

        public abstract Envelope GetEnvelope();

        public abstract void Dispose();
    }
}
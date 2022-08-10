using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace ZMap.Source
{
    public abstract class RasterSource : IRasterSource
    {
        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int SRID { get; set; }

        /// <summary>
        /// 数据源读出的数据， 需要转码成图片
        /// </summary>
        /// <param name="extent"></param>
        /// <returns></returns>
        public abstract Task<byte[]> GetImageInExtentAsync(Envelope extent);

        public void SetFilter(IFeatureFilter filter)
        {
        }

        public abstract Envelope GetEnvelope();

        public abstract void Dispose();
    }
}
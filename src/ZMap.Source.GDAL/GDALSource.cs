using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using OSGeo.GDAL;

namespace ZMap.Source.GDAL
{
    public class GDALSource : RasterSource
    {
        private static readonly ConcurrentDictionary<string, dynamic>
            Cache = new();

        /// <summary>
        /// 文件路径
        /// </summary>
        public string File { get; }

        public string Type { get; }

        public RasterMetadata Metadata { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="ArgumentException"></exception>
        public GDALSource(string file)
        {
            File = file;
            if (!System.IO.File.Exists(File))
            {
                throw new ArgumentException($"文件 {file} 不存在");
            }

            Type = Path.GetExtension(file);

            Metadata = Cache.GetOrAdd(File, f =>
            {
                var metadata = new RasterMetadata();

                var dataset = Gdal.OpenShared(f, Access.GA_ReadOnly);
                metadata.Projection = dataset.GetProjectionRef();
                using var p = new OSGeo.OSR.SpatialReference(null);
                var wkt = metadata.Projection;
                var res = p.ImportFromWkt(ref wkt);
                var srid = 0;
                if (res == 0)
                {
                    if (metadata.Projection.StartsWith("PROJCS"))
                        int.TryParse(p.GetAuthorityCode("PROJCS"), out srid);
                    else if (metadata.Projection.StartsWith("GEOGCS"))
                        int.TryParse(p.GetAuthorityCode("GEOGCS"), out srid);
                    else
                        srid = p.AutoIdentifyEPSG();
                }

                var index = new STRtree<string>();
                Envelope envelope;

                var metadataDomainList = new HashSet<string>(dataset.GetMetadataDomainList());
                if (metadataDomainList.Contains("SUBDATASETS"))
                {
                    var subDataSets = dataset.GetMetadata("SUBDATASETS");

                    envelope = new Envelope();
                    foreach (var t in subDataSets)
                    {
                        var namePos = t.IndexOf("_NAME=", StringComparison.Ordinal);
                        if (namePos < 0) continue;

                        var subDataSet = t.Substring(namePos + 6);
                        using var ds = Gdal.Open(subDataSet, Access.GA_ReadOnly);
                        var tmp = GetEnvelope(ds);
                        index.Insert(tmp, ds.GetDescription());
                        envelope.ExpandToInclude(tmp);
                    }
                }
                else
                {
                    envelope = GetEnvelope(dataset);
                }

                metadata.Dataset = dataset;
                metadata.SRID = srid;
                metadata.Width = dataset.RasterXSize;
                metadata.Height = dataset.RasterYSize;
                metadata.Index = index;
                metadata.Extent = envelope;
                metadata.Bands = dataset.RasterCount;
                metadata.Transform = new GeoTransform(dataset);
                return metadata;
            });
        }

        /// <summary>
        /// Read image data with BBox
        /// </summary>
        /// <param name="extent">Image extent in geo space</param>
        /// <returns>Image data</returns>
        public override Task<byte[]> GetImageInExtentAsync(Envelope extent)
        {
            if (!Metadata.Extent.Contains(extent))
            {
                return null;
            }

            var (xOff, yOff, xSize, ySize) = GroundToImage(extent);

            var ds = Metadata.Dataset;
            var result = Enumerable
                .Range(1, ds.RasterCount)
                .SelectMany(i => Read(ds.GetRasterBand(i), xOff, yOff, xSize, ySize, xSize, ySize, 0, 0))
                .ToArray();

            return Task.FromResult(result);
        }

        public override Envelope GetEnvelope()
        {
            return null;
        }

        public override void Dispose()
        {
            Metadata?.Dispose();
        }

        public override string ToString()
        {
            return $"{File} {Type}";
        }

        /// <summary>
        /// Read raster data from band
        /// </summary>
        /// <param name="band">Band of raster</param>
        /// <param name="xOff">Pixel offset at x axis</param>
        /// <param name="yOff">Pixel offset at y axis</param>
        /// <param name="xSize">Pixel width</param>
        /// <param name="ySize">Pixel height</param>
        /// <param name="bufXSize">Pixel width of readed out image</param>
        /// <param name="bufYSize">Pixel height of readed out image</param>
        /// <param name="pixelSpace"></param>
        /// <param name="lineSpace"></param>
        /// <returns>Raster data</returns>
        private static IEnumerable<byte> Read(Band band, int xOff, int yOff, int xSize, int ySize, int bufXSize,
            int bufYSize,
            int pixelSpace, int lineSpace)
        {
            byte[] result = null;
            var dataType = band.DataType;

            switch (dataType)
            {
                case DataType.GDT_Byte:
                {
                    var byteBuffer = new byte[xSize * ySize];
                    band.ReadRaster(xOff, yOff, xSize, ySize, byteBuffer, bufXSize, bufYSize, pixelSpace, lineSpace);

                    result = byteBuffer;
                    break;
                }
                case DataType.GDT_UInt16:
                case DataType.GDT_Int16:
                {
                    var shortBuffer = new short[xSize * ySize];
                    band.ReadRaster(xOff, yOff, xSize, ySize, shortBuffer, bufXSize, bufYSize, pixelSpace, lineSpace);

                    result = ToByteArray(shortBuffer);
                    break;
                }
                case DataType.GDT_UInt32:
                case DataType.GDT_Int32:
                {
                    var intBuffer = new int[xSize * ySize];
                    band.ReadRaster(xOff, yOff, xSize, ySize, intBuffer, bufXSize, bufYSize, pixelSpace, lineSpace);

                    result = ToByteArray(intBuffer);
                    break;
                }
                case DataType.GDT_Float32:
                {
                    var singleBuffer = new Single[xSize * ySize];
                    band.ReadRaster(xOff, yOff, xSize, ySize, singleBuffer, bufXSize, bufYSize, pixelSpace, lineSpace);

                    result = ToByteArray(singleBuffer);
                    break;
                }
                case DataType.GDT_Float64:
                {
                    var floatBuffer = new Single[xSize * ySize];
                    band.ReadRaster(xOff, yOff, xSize, ySize, floatBuffer, bufXSize, bufYSize, pixelSpace, lineSpace);
                    result = ToByteArray(floatBuffer);
                    break;
                }
                case DataType.GDT_Unknown:
                    break;
                case DataType.GDT_CInt16:
                    break;
                case DataType.GDT_CInt32:
                    break;
                case DataType.GDT_CFloat32:
                    break;
                case DataType.GDT_CFloat64:
                    break;
                case DataType.GDT_TypeCount:
                    break;
                default:
                    //todo See https://blog.csdn.net/zhuimengshizhe87/article/details/19427323
                    throw new NotSupportedException("GDT_Unkonwn or complex type(eg CInt CFloat) not supported now.");
            }

            return result;
        }

        private static byte[] ToByteArray<T>(T[] arr) where T : struct
        {
            var type = typeof(T);
            int typeSize;

            if (type == typeof(short))
            {
                typeSize = sizeof(short);
            }
            else if (type == typeof(int))
            {
                typeSize = sizeof(int);
            }
            else if (type == typeof(Single))
            {
                typeSize = sizeof(Single);
            }
            else if (type == typeof(float))
            {
                typeSize = sizeof(float);
            }
            else
            {
                throw new NotSupportedException("Only support short,int,single,float now.");
            }

            var result = new byte[arr.Length * typeSize];
            Buffer.BlockCopy(arr, 0, result, 0, result.Length);

            return result;
        }

        private Envelope GetEnvelope(Dataset dataset)
        {
            // no rotation...use default transform
            var geoTransform = new GeoTransform(dataset);
            if (geoTransform.IsIdentity)
                geoTransform = new GeoTransform(-0.5, dataset.RasterYSize + 0.5);

            // image pixels
            double dblW = dataset.RasterXSize;
            double dblH = dataset.RasterYSize;

            var left = geoTransform.EnvelopeLeft(dblW, dblH);
            var right = geoTransform.EnvelopeRight(dblW, dblH);
            var top = geoTransform.EnvelopeTop(dblW, dblH);
            var bottom = geoTransform.EnvelopeBottom(dblW, dblH);

            return new Envelope(left, right, bottom, top);
        }

        /// <summary>
        /// Caculate origin,width and height in pixel space when reading raster image with a 2D BBox
        /// </summary>
        /// <param name="extent">Image extent(geo space)</param>
        /// <returns>Image origin, width and height in pixel space</returns>
        private (int xOff, int yOff, int xSize, int ySize) GroundToImage(Envelope extent)
        {
            var leftUpPixel = Metadata.Transform.GroundToImage(new Coordinate(extent.MinX, extent.MaxY));
            var rightDownPixel = Metadata.Transform.GroundToImage(new Coordinate(extent.MaxX, extent.MinY));

            var xSize = Math.Abs((int)(rightDownPixel.X - leftUpPixel.X));
            var ySize = Math.Abs((int)(leftUpPixel.Y - rightDownPixel.Y));
            var xOff = (int)leftUpPixel.X;
            var yOff = (int)leftUpPixel.Y;

            return (xOff, yOff, xSize, ySize);
        }
    }
}
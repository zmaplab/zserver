using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.IO;
using NetTopologySuite.IO.ShapeFile.Extended;
using ProjNet.CoordinateSystems;
using ZMap.Indexing;
using ZMap.Infrastructure;

namespace ZMap.Source.ShapeFile
{
    public class ShapeFileSource : VectorSourceBase
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string File { get; }

        private static readonly CoordinateSystemFactory CoordinateSystemFactory = new();

        private readonly GeometryFactory _geometryFactory;

        // ReSharper disable once InconsistentNaming
        public ShapeFileSource(string file)
        {
            File = new FileInfo(file).FullName;
            Srid = GetSrid(file);
            _geometryFactory =
                NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(), Srid);
        }

        public override ISource Clone()
        {
            return (ISource)MemberwiseClone();
        }

        public override Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent)
        {
            var features = new List<Feature>();

            var predicate = Filter?.ToPredicate();
            FileReader tuple;
            lock (typeof(ShapeFileSource))
            {
                tuple = Cache.GetOrCreate(File,
                    entry =>
                    {
                        var mmf = MemoryMappedFile.CreateFromFile(File, FileMode.Open);
                        var stream = mmf.CreateViewStream();
                        var reader = new BinaryReader(stream, Encoding.Unicode);
                        var spatialIndex = SpatialIndexFactory.Create(File, reader, SpatialIndexType.STRTree);

                        var result = new FileReader
                        {
                            Reader = reader,
                            SpatialIndex = spatialIndex
                        };
                        entry.SetValue(result);
                        entry.SetSlidingExpiration(TimeSpan.FromDays(365));
                        return result;
                    });
            }

            var spatialIndexItems = GetObjectIDsInView(tuple.SpatialIndex, extent);
            if (spatialIndexItems.Count == 0)
            {
                return Task.FromResult<IEnumerable<Feature>>(Array.Empty<Feature>());
            }

            // ISpatialIndex<SpatialIndexEntry> _tree;
            // 不能使用 using，会关闭流

            using var dbaseFile = new DbaseReader(Path.ChangeExtension(File, ".dbf"));

            foreach (var spatialIndexItem in spatialIndexItems)
            {
                var feature = GetOrCreate(extent, tuple.Reader, dbaseFile, spatialIndexItem);

                if (predicate != null)
                {
                    if (predicate(feature))
                    {
                        features.Add(feature);
                    }
                }
                else
                {
                    features.Add(feature);
                }
            }

            return Task.FromResult<IEnumerable<Feature>>(features);
        }

        public override Envelope GetEnvelope()
        {
            return null;
        }

        public override string ToString()
        {
            return $"Path: {File}";
        }

        public override void Dispose()
        {
        }

        public Func<Feature, bool> Predicate { get; set; }

        private Feature GetOrCreate(Envelope queryExtent, BinaryReader binaryReader, DbaseReader dbaseFile,
            SpatialIndexItem spatialIndexItem)
        {
            var attributes = GetAttribute(dbaseFile, (int)spatialIndexItem.Index);
            var geometry = ReadGeometry(spatialIndexItem, binaryReader);

            if (geometry == null
                || !geometry.IsValid
                || !queryExtent.Intersects(geometry.EnvelopeInternal))
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            foreach (var name in attributes.GetNames())
            {
                var value = attributes[name];
                if (value is string v)
                {
                    value = v.Replace("\0", "").Trim();
                }

                dict.Add(name, value);
            }

            var feature = new Feature(geometry, dict);
            return feature;
        }

        private IAttributesTable GetAttribute(DbaseReader dbaseFile, int index)
        {
            return dbaseFile.ReadEntry(index);
        }

        private int GetSrid(string path)
        {
            var projPath = path.Replace(".shp", ".prj");
            CoordinateSystem coordinateSystem;
            try
            {
                if (!System.IO.File.Exists(projPath))
                {
                    throw new Exception($"投影文件不存在 {projPath}");
                }

                coordinateSystem =
                    CoordinateSystemFactory.CreateFromWkt(System.IO.File.ReadAllText(Path.Combine(projPath)));
            }
            catch
            {
                throw new Exception($"投影文件不正确 {projPath}");
            }

            return (int)coordinateSystem.AuthorityCode;
        }

        private List<SpatialIndexItem> GetObjectIDsInView(ISpatialIndex<SpatialIndexItem> tree, Envelope bbox)
        {
            //Use the spatial index to get a list of features whose boundingBox intersects bbox
            var res = tree.Query(bbox);

            /*Sort oids so we get a forward only read of the shapefile*/
            var ret = new List<SpatialIndexItem>(res);
            ret.Sort((a, b) => (int)(a.Index - b.Index));

            return ret;
        }

        private Geometry ReadGeometry(SpatialIndexItem item, BinaryReader reader)
        {
            lock (typeof(ShapeFileSource))
            {
                var diff = item.Offset - reader.BaseStream.Position;
                reader.BaseStream.Seek(diff, SeekOrigin.Current);

                //Skip record number
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                var bytes = reader.ReadBytes(item.Length);
                using var geometryReader = new BigEndianBinaryReader(new MemoryStream(bytes));

                var typeValue = geometryReader.ReadInt32();
                if (typeValue == 0)
                {
                    return null;
                }

                var type = (ShapeGeometryType)typeValue;
                var handler = Shapefile.GetShapeHandler(type);

                geometryReader.BaseStream.Seek(0, 0);

                var geometry = handler.Read(geometryReader, item.Length, _geometryFactory);
                return geometry;
            }
        }

        record FileReader
        {
            public BinaryReader Reader { get; init; }
            public ISpatialIndex<SpatialIndexItem> SpatialIndex { get; init; }
        }
    }
}
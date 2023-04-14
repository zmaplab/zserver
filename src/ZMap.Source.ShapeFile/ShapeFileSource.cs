using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<string, Func<Feature, bool>>
            FilterCache = new();

        private readonly GeometryFactory _geometryFactory;

        // ReSharper disable once InconsistentNaming
        public ShapeFileSource(string file)
        {
            File = new FileInfo(file).FullName;
            SRID = GetSrid(file);
            _geometryFactory =
                NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(), SRID);
        }

        record FileReader
        {
            public BinaryReader Reader { get; set; }
            public ISpatialIndex<SpatialIndexEntry> Tree { get; set; }
        }

        public override ValueTask<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent)
        {
            var features = new List<Feature>();

            var filterFunc = Filter?.GetFunc();

            var tuple = Cache.GetOrCreate(File,
                entry =>
                {
                    var mmf = MemoryMappedFile.CreateFromFile(File, FileMode.Open);

                    var stream = mmf.CreateViewStream();
                    var reader = new BinaryReader(stream, Encoding.Unicode);
                    var tree = SpatialIndexFactory.Create(File, reader, SpatialIndexType.STRTree);

                    var result = new FileReader
                    {
                        Reader = reader,
                        Tree = tree
                    };
                    entry.SetValue(result);
                    entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                    return result;
                });

            var spatialIndexItems = GetObjectIDsInView(tuple.Tree, extent);
            if (spatialIndexItems.Count == 0)
            {
                return new ValueTask<IEnumerable<Feature>>(Array.Empty<Feature>());
            }

            // ISpatialIndex<SpatialIndexEntry> _tree;
            // 不能使用 using，会关闭流

            using var dbaseFile = new DbaseReader(Path.ChangeExtension(File, ".dbf"));

            foreach (var spatialIndexItem in spatialIndexItems)
            {
                var feature = GetOrCreate(extent, tuple.Reader, dbaseFile, spatialIndexItem);

                if (filterFunc != null)
                {
                    if (filterFunc(feature))
                    {
                        features.Add(feature);
                    }
                }
                else
                {
                    features.Add(feature);
                }
            }

            return new ValueTask<IEnumerable<Feature>>(features);
        }

        public override Envelope GetEnvelope()
        {
            return null;
        }

        private Feature GetOrCreate(Envelope queryExtent, BinaryReader binaryReader, DbaseReader dbaseFile,
            SpatialIndexEntry spatialIndexEntry)
        {
            return Cache.GetOrCreate($"ShapeFile:{File}:{spatialIndexEntry.Index}", entry =>
            {
                var attributes = GetAttribute(dbaseFile, (int)spatialIndexEntry.Index);
                var geometry = ReadGeometry(spatialIndexEntry, binaryReader);

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

                var f = new Feature(geometry, dict);
                entry.SetValue(f);
                entry.SetAbsoluteExpiration(TimeSpan.FromDays(12));
                return f;
            });
        }

        private IAttributesTable GetAttribute(DbaseReader dbaseFile, int index)
        {
            return dbaseFile.ReadEntry(index);
        }

        public override string ToString()
        {
            return $"Path: {File}";
        }

        public override void Dispose()
        {
        }

        public Func<Feature, bool> Predicate { get; set; }

        private int GetSrid(string path)
        {
            var prjPath = path.Replace(".shp", ".prj");
            CoordinateSystem coordinateSystem;
            try
            {
                if (!System.IO.File.Exists(prjPath))
                {
                    throw new Exception($"投影文件不存在 {prjPath}");
                }

                coordinateSystem =
                    CoordinateSystemFactory.CreateFromWkt(System.IO.File.ReadAllText(Path.Combine(prjPath)));
            }
            catch
            {
                throw new Exception($"投影文件不正确 {prjPath}");
            }

            return (int)coordinateSystem.AuthorityCode;
        }

        private List<SpatialIndexEntry> GetObjectIDsInView(ISpatialIndex<SpatialIndexEntry> tree, Envelope bbox)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            var res = tree.Query(bbox);

            /*Sort oids so we get a forward only read of the shapefile*/
            var ret = new List<SpatialIndexEntry>(res);
            ret.Sort((a, b) => (int)(a.Index - b.Index));

            return ret;
        }

        private Geometry ReadGeometry(SpatialIndexEntry entry, BinaryReader reader)
        {
            lock (typeof(ShapeFileSource))
            {
                var diff = entry.Offset - reader.BaseStream.Position;
                reader.BaseStream.Seek(diff, SeekOrigin.Current);

                //Skip record number
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                var bytes = reader.ReadBytes(entry.Length);
                using var geometryReader = new BigEndianBinaryReader(new MemoryStream(bytes));

                var typeValue = geometryReader.ReadInt32();
                if (typeValue == 0)
                {
                    return null;
                }

                var type = (ShapeGeometryType)typeValue;
                var handler = Shapefile.GetShapeHandler(type);

                geometryReader.BaseStream.Seek(0, 0);

                var geometry = handler.Read(geometryReader, entry.Length, _geometryFactory);
                return geometry;
            }
        }
    }
}
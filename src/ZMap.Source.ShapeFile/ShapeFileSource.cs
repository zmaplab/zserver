using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.IO;
using NetTopologySuite.IO.ShapeFile.Extended;
using ProjNet.CoordinateSystems;
using ZMap.Indexing;
using ZMap.Utilities;

namespace ZMap.Source.ShapeFile
{
    public class ShapeFileSource : VectorSourceBase
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string File { get; }

        /// <summary>
        /// 启用 MMF
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool EnableMemoryMappedFile { get; }

        private static readonly CoordinateSystemFactory CoordinateSystemFactory = new();

        private static readonly ConcurrentDictionary<string, Func<Feature, bool>>
            FilterCache = new();

        private readonly ISpatialIndex<SpatialIndexEntry> _tree;
        private readonly GeometryFactory _geometryFactory;

        private static readonly IMemoryCache MemoryCache =
            new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        // ReSharper disable once InconsistentNaming
        public ShapeFileSource(string file, bool enableMMF)
        {
            File = file;
            SRID = GetSrid(file);
            EnableMemoryMappedFile = enableMMF;
            _geometryFactory =
                NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(), SRID);

            _tree = new SpatialIndexFactory().TryCreate(file, SpatialIndexType.STRTree);
        }

        public override ValueTask<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope extent)
        {
            var features = new List<Feature>();

            Func<Feature, bool> filterFunc = Filter.GetFunc();

            var spatialIndexItems = GetObjectIDsInView(extent);
            if (spatialIndexItems.Count == 0)
            {
                return new ValueTask<IEnumerable<Feature>>(Array.Empty<Feature>());
            }

            var tuple = MemoryCache.GetOrCreate(File, _ =>
            {
                if (!EnableMemoryMappedFile)
                {
                    var binaryReader = new BinaryReader(new MemoryStream(System.IO.File.ReadAllBytes(File)));
                    var dbaseFile = new DbaseReader(Path.ChangeExtension(File, ".dbf"));
                    return (binaryReader, dbaseFile);
                }
                else
                {
                    var mmFile = MemoryMappedFile.CreateFromFile(File, FileMode.Open);
                    var stream = mmFile.CreateViewStream();
                    var binaryReader = new BinaryReader(stream);
                    var dbaseFile = new DbaseReader(Path.ChangeExtension(File, ".dbf"));
                    return (binaryReader, dbaseFile);
                }
            });

            foreach (var spatialIndexItem in spatialIndexItems)
            {
                var feature = GetOrCreate(extent, tuple.binaryReader, tuple.dbaseFile, spatialIndexItem);

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
            return MemoryCache.GetOrCreate($"{File}:{spatialIndexEntry.Index}", entry =>
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

            return CoordinateSystemUtilities.GetSRID(coordinateSystem.Name.ToUpperInvariant());
        }

        private List<SpatialIndexEntry> GetObjectIDsInView(Envelope bbox)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            var res = _tree.Query(bbox);

            /*Sort oids so we get a forward only read of the shapefile*/
            var ret = new List<SpatialIndexEntry>(res);
            ret.Sort((a, b) => (int)(a.Index - b.Index));

            return ret;
        }

        private Geometry ReadGeometry(SpatialIndexEntry entry, BinaryReader brGeometryStream)
        {
            var diff = entry.Offset - brGeometryStream.BaseStream.Position;
            brGeometryStream.BaseStream.Seek(diff, SeekOrigin.Current);

            //Skip record number
            brGeometryStream.BaseStream.Seek(8, SeekOrigin.Current);

            var bytes = brGeometryStream.ReadBytes(entry.RecordLength);
            using var geometryReader = new BigEndianBinaryReader(new MemoryStream(bytes));

            var typeValue = geometryReader.ReadInt32();
            if (typeValue == 0)
            {
                return null;
            }

            var type = (ShapeGeometryType)typeValue;
            var handler = Shapefile.GetShapeHandler(type);

            geometryReader.BaseStream.Seek(0, 0);

            var geometry = handler.Read(geometryReader, entry.RecordLength, _geometryFactory);
            return geometry;
        }

        public override void Dispose()
        {
        }

        public Func<Feature, bool> Predicate { get; set; }
    }
}
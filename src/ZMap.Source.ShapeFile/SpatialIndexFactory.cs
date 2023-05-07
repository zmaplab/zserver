using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using ZMap.Indexing;
using ZMap.Infrastructure;

namespace ZMap.Source.ShapeFile
{
    public static class SpatialIndexFactory
    {
        private static readonly MessagePackSerializerOptions SerializerOptions =
            MessagePackSerializer.Typeless.DefaultOptions.WithCompression(MessagePackCompression.Lz4Block);

        public static ISpatialIndex<SpatialIndexEntry> Create(string shapeFile, BinaryReader reader,
            SpatialIndexType type)
        {
            ISpatialIndex<SpatialIndexEntry> tree;

            var sidxPath = Path.ChangeExtension(shapeFile, ".sidx");

            if (!File.Exists(sidxPath))
            {
                var entries = GetAllFeatureBoundingBoxes(reader).ToList();
                tree = CreateSpatialIndex(type, entries);
                Save(entries, sidxPath);
            }
            else
            {
                tree = Load(sidxPath, type);
            }

            return tree;
        }

        private static void Save(IEnumerable<SpatialIndexEntry> entries, string path)
        {
            File.WriteAllBytes(path, MessagePackSerializer.Serialize(entries, SerializerOptions));
        }

        public static ISpatialIndex<SpatialIndexEntry> Load(string filename, SpatialIndexType type)
        {
            using var stream = File.OpenRead(filename);
            var entries = MessagePackSerializer.Deserialize<IEnumerable<SpatialIndexEntry>>(stream,
                SerializerOptions);
            return CreateSpatialIndex(type, entries);
        }

        private static int SwapByteOrder(int i)
        {
            var buffer = BitConverter.GetBytes(i);
            Array.Reverse(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Reads all boundingboxes of features in the shapefile. This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<SpatialIndexEntry> GetAllFeatureBoundingBoxes(BinaryReader shapeFileReader)
        {
            // var headerBuffer = new byte[100];
            // shapeFileStream.Read(headerBuffer, 0, 100);
            var shapeFileStream = shapeFileReader.BaseStream;
            shapeFileStream.Seek(100, SeekOrigin.Begin);

            for (var a = 0; a < int.MaxValue; ++a)
            {
                if (shapeFileStream.Position >= shapeFileStream.Length ||
                    shapeFileStream.Length - shapeFileStream.Position < 24)
                {
                    Log.Logger.LogInformation($"Read end stream at: {shapeFileStream.Position}, stream length: {shapeFileStream.Length}");
                    break;
                }

                var recordOffset = (int)shapeFileStream.Position;

                // Get the oid
                shapeFileReader.ReadInt32();

                // Get the record length
                var recordLength = 2 * SwapByteOrder(shapeFileReader.ReadInt32()); // 108

                var typeValue = shapeFileReader.ReadInt32(); // 112
                if (typeValue <= 0)
                {
                    continue;
                }

                var shapeType = (ShapeGeometryType)typeValue;
                if (shapeType == ShapeGeometryType.Point || shapeType == ShapeGeometryType.PointZ ||
                    shapeType == ShapeGeometryType.PointM)
                {
                    var x1 = shapeFileReader.ReadDouble();
                    var y1 = shapeFileReader.ReadDouble();

                    shapeFileStream.Seek(recordLength - 20, SeekOrigin.Current);

                    yield return new SpatialIndexEntry
                    {
                        Index = (uint)a /*+1*/,
                        Offset = recordOffset,
                        Length = recordLength,
                        X1 = x1,
                        Y1 = y1,
                        X2 = x1,
                        Y2 = y1
                    };
                }
                else
                {
                    var x1 = shapeFileReader.ReadDouble();
                    var y1 = shapeFileReader.ReadDouble();
                    var x2 = shapeFileReader.ReadDouble();
                    var y2 = shapeFileReader.ReadDouble();
                    shapeFileStream.Seek(recordLength - 36, SeekOrigin.Current);
                    yield return new SpatialIndexEntry
                    {
                        Index = (uint)a /*+1*/,
                        Offset = recordOffset,
                        Length = recordLength,
                        X1 = x1,
                        X2 = x2,
                        Y1 = y1,
                        Y2 = y2
                    };
                }
            }
        }

        /// <summary>
        /// Generates a spatial index for a specified shape file.
        /// </summary>
        private static ISpatialIndex<SpatialIndexEntry> CreateSpatialIndex(SpatialIndexType type,
            IEnumerable<SpatialIndexEntry> entries)
        {
            ISpatialIndex<SpatialIndexEntry> tree = type switch
            {
                SpatialIndexType.Quadtree => new Quadtree<SpatialIndexEntry>(),
                SpatialIndexType.STRTree => new STRtree<SpatialIndexEntry>(),
                SpatialIndexType.HPRTree => new HPRtree<SpatialIndexEntry>(),
                _ => throw new ArgumentException("unsupported index type")
            };

            foreach (var entry in entries)
            {
                var box = new Envelope(entry.X1, entry.X2, entry.Y1, entry.Y2);
                if (box.IsNull)
                {
                    continue;
                }

                tree.Insert(box, entry);
            }

            return tree;
        }
    }
}
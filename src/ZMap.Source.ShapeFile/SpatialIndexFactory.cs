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

        public static ISpatialIndex<SpatialIndexItem> Create(string shapeFile, BinaryReader reader,
            SpatialIndexType type)
        {
            ISpatialIndex<SpatialIndexItem> tree;

            var sidxPath = Path.ChangeExtension(shapeFile, ".sidx");

            if (!File.Exists(sidxPath))
            {
                List<SpatialIndexItem> entries;
                try
                {
                    entries = GetAllFeatureBoundingBoxes(reader).ToList();
                }
                catch (Exception e)
                {
                    Log.Logger.LogError("加载空间索引失败， 矢量文件： {ShapeFile}， 异常： {Exception}", shapeFile, e.ToString());
                    entries = new List<SpatialIndexItem>();
                }

                tree = CreateSpatialIndex(type, entries);
                Save(entries, sidxPath);
            }
            else
            {
                tree = Load(sidxPath, type);
            }

            return tree;
        }

        private static void Save(IEnumerable<SpatialIndexItem> entries, string path)
        {
            File.WriteAllBytes(path, MessagePackSerializer.Serialize(entries, SerializerOptions));
        }

        public static ISpatialIndex<SpatialIndexItem> Load(string filename, SpatialIndexType type)
        {
            using var stream = File.OpenRead(filename);
            var entries = MessagePackSerializer.Deserialize<IEnumerable<SpatialIndexItem>>(stream,
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
        /// Reads all boundingBoxes of features in the shapefile. This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<SpatialIndexItem> GetAllFeatureBoundingBoxes(BinaryReader shapeFileReader)
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
                    Log.Logger.LogInformation("Read end stream at: {Position}, stream length: {Length}",
                        shapeFileStream.Position, shapeFileStream.Length);
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

                    if (double.IsNaN(x1) || double.IsNaN(y1))
                    {
                        continue;
                    }

                    yield return new SpatialIndexItem
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

                    if (double.IsNaN(x1) || double.IsNaN(x2) || double.IsNaN(y1) || double.IsNaN(y2))
                    {
                        continue;
                    }

                    yield return new SpatialIndexItem
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
        private static ISpatialIndex<SpatialIndexItem> CreateSpatialIndex(SpatialIndexType type,
            IEnumerable<SpatialIndexItem> entries)
        {
            ISpatialIndex<SpatialIndexItem> tree = type switch
            {
                SpatialIndexType.Quadtree => new Quadtree<SpatialIndexItem>(),
                SpatialIndexType.STRTree => new STRtree<SpatialIndexItem>(),
                SpatialIndexType.HPRTree => new HPRtree<SpatialIndexItem>(),
                _ => throw new ArgumentException("unsupported index type")
            };

            foreach (var entry in entries)
            {
                try
                {
                    var box = new Envelope(entry.X1, entry.X2, entry.Y1, entry.Y2);
                    if (box.IsNull)
                    {
                        continue;
                    }

                    tree.Insert(box, entry);
                }
                catch (Exception e)
                {
                }
            }

            return tree;
        }
    }
}
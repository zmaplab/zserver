using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using ZMap.Indexing;

namespace ZMap.Source.ShapeFile
{
    public class SpatialIndexFactory
    {
        public ISpatialIndex<SpatialIndexEntry> TryCreate(string shapeFile, SpatialIndexType type)
        {
            return CreateSpatialIndex(type, GetAllFeatureBoundingBoxes(shapeFile));
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
        private IEnumerable<SpatialIndexEntry> GetAllFeatureBoundingBoxes(string shapeFile)
        {
            using var shapeFileStream = new FileStream(shapeFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            shapeFileStream.Position = 100;

            using var shapeFileReader = new BinaryReader(shapeFileStream, Encoding.Unicode);

            for (var a = 0; a < int.MaxValue; ++a)
            {
                if (shapeFileStream.Position >= shapeFileStream.Length)
                {
                    break;
                }

                var recordOffset = (int)shapeFileStream.Position;

                // Get the oid
                shapeFileReader.ReadInt32();

                // Get the record length
                var recordLength = 2 * SwapByteOrder(shapeFileReader.ReadInt32()); // 108

                var typeValue = shapeFileReader.ReadInt32(); // 112
                if (typeValue == 0)
                {
                    continue;
                }

                var shapeType = (ShapeGeometryType)typeValue;
                if (shapeType == ShapeGeometryType.Point || shapeType == ShapeGeometryType.PointZ ||
                    shapeType == ShapeGeometryType.PointM)
                {
                    var coordinate = new Coordinate(shapeFileReader.ReadDouble(), shapeFileReader.ReadDouble());

                    shapeFileStream.Seek(recordLength - 20, SeekOrigin.Current);
                    yield return new SpatialIndexEntry
                    {
                        Index = (uint)a /*+1*/,
                        Offset = recordOffset,
                        RecordLength = recordLength,
                        Box = new Envelope(coordinate)
                    };
                }
                else
                {
                    var c1 = new Coordinate(shapeFileReader.ReadDouble(), shapeFileReader.ReadDouble());
                    var c2 = new Coordinate(shapeFileReader.ReadDouble(), shapeFileReader.ReadDouble());
                    shapeFileStream.Seek(recordLength - 36, SeekOrigin.Current);
                    yield return new SpatialIndexEntry
                    {
                        Index = (uint)a /*+1*/,
                        Offset = recordOffset,
                        RecordLength = recordLength,
                        Box = new Envelope(c1, c2)
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
                if (entry.Box.IsNull)
                {
                    continue;
                }

                tree.Insert(entry.Box, entry);
            }

            return tree;
        }
    }
}
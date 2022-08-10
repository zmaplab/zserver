using System;
using NetTopologySuite.Geometries;

namespace ZMap.Indexing
{
    [Serializable]
    public record SpatialIndexEntry
    {
        public Envelope Box { get; set; }

        /// <summary>
        /// Feature ID
        /// </summary>
        public uint Index { get; set; }

        public int Offset { get; set; }

        public int RecordLength { get; set; }
    }
}
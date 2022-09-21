using System;
using MessagePack;
using NetTopologySuite.Geometries;

namespace ZMap.Indexing
{
    [Serializable]
    [MessagePackObject]
    public record SpatialIndexEntry
    {
        /// <summary>
        /// 
        /// </summary>
        [Key(0)]
        public double X1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(1)]
        public double X2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(2)]
        public double Y1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(3)]
        public double Y2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(4)]
        public uint Index { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(5)]
        public int Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(6)]
        public int RecordLength { get; set; }
    }
}
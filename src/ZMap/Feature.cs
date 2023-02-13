using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ZMap.Utilities;

namespace ZMap
{
    public record Feature
    {
        private readonly IDictionary<string, dynamic> _attributes;
        private IReadOnlyDictionary<string, dynamic> _environments;

        public Feature(Geometry geometry, IDictionary<string, dynamic> attributes)
        {
            Geometry = geometry;
            _attributes = attributes;
        }

        public Feature(string geomColumn, IDictionary<string, dynamic> attributes)
        {
            _attributes = attributes;

            if (attributes == null || !attributes.ContainsKey(geomColumn))
            {
                return;
            }

            Geometry = attributes[geomColumn];
            attributes.Remove(geomColumn);
        }

        /// <summary>
        /// 是否空图斑
        /// </summary>
        public bool IsEmpty => Geometry == null || Geometry.IsEmpty;

        /// <summary>
        /// 图形
        /// </summary>
        public Geometry Geometry { get; private set; }

        public dynamic this[string name] =>
            _attributes == null ? null : _attributes.ContainsKey(name) ? _attributes[name] : null;

        public void SetEnvironment(IReadOnlyDictionary<string, dynamic> environments)
        {
            _environments = environments ?? new Dictionary<string, dynamic>();
        }

        public dynamic GetEnvValue(string name)
        {
            return _environments.TryGetValue(name, out var v) ? v : null;
        }

        public IDictionary<string, dynamic> GetAttributes() => _attributes;

        /// <summary>
        /// 坐标系转换
        /// </summary>
        /// <param name="transformation"></param>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Feature Transform(ICoordinateTransformation transformation)
        {
            Geometry = CoordinateReferenceSystem.Transform(Geometry, transformation);
            return this;
        }

        public void Simplify()
        {
            // todo: 哪些情况不能简化？
            if (Geometry.OgcGeometryType == OgcGeometryType.Point)
            {
                return;
            }

            var srid = Geometry.SRID;
            var coordinateSystem = CoordinateReferenceSystem.Get(srid);
            if (coordinateSystem == null)
            {
                return;
            }

            // 84 最小可用精度是 0.00001 度， 0.0001 以上图形异常
            // 大地坐标系最大可用精度是 0.0001 米， 0.00001 开始会消除不掉异常点
            var distanceTolerance = coordinateSystem is GeographicCoordinateSystem ? 0.00001 : 0.0001;
            Geometry = VWSimplifier.Simplify(Geometry, distanceTolerance);
        }
    }
}
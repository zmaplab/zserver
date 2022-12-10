using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using ProjNet.CoordinateSystems;

namespace ZMap.Utilities
{
    public static class CoordinateSystemUtilities
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<int, CoordinateSystem> SRIDCache;
        private static readonly ConcurrentDictionary<string, int> NamedCache;

        static CoordinateSystemUtilities()
        {
            SRIDCache = new ConcurrentDictionary<int, CoordinateSystem>();
            NamedCache = new ConcurrentDictionary<string, int>();

            var file = Path.Combine(AppContext.BaseDirectory, "proj.xml");
            var coordinateSystemFactory = new CoordinateSystemFactory();
            var xml = new XmlDocument();
            xml.Load(file);
            var nodes = xml.DocumentElement?.SelectNodes("/SpatialReference/*");
            if (nodes != null)
            {
                foreach (XmlNode referenceNode in nodes)
                {
                    var sridNode = referenceNode?.SelectSingleNode("SRID");
                    if (sridNode == null)
                    {
                        continue;
                    }

                    var wkt = referenceNode.LastChild?.InnerText;

                    if (!string.IsNullOrWhiteSpace(wkt) && int.TryParse(sridNode.InnerText, out var srid))
                    {
                        var coordinateSystem = coordinateSystemFactory.CreateFromWkt(wkt);
                        SRIDCache.TryAdd(srid, coordinateSystem);

                        var name = coordinateSystem.Name.Replace(" / ", "_").Replace("-", "_").Replace(" ", "_")
                            .Replace("Gauss_Kruger", "GK").ToUpperInvariant();
                        NamedCache.TryAdd(name, srid);
                    }
                }
            }
        }

        public static CoordinateSystem Get(int srid)
        {
            return srid switch
            {
                4326 => GeographicCoordinateSystem.WGS84,
                3857 => ProjectedCoordinateSystem.WebMercator,
                _ => SRIDCache.TryGetValue(srid, out var coordinateSystem) ? coordinateSystem : null
            };
        }
    }
}
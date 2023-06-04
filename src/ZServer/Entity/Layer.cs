using System.Collections.Generic;
using NetTopologySuite.Geometries;
using ZMap;
using ZMap.Source;
using ZMap.Style;

namespace ZServer.Entity
{
    public class LayerEntity : Layer, IOgcWebServiceProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public HashSet<ServiceType> Services { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ResourceGroup ResourceGroup { get; set; }

        public override string GetResourceGroupName()
        {
            return ResourceGroup?.Name;
        }

        public LayerEntity(ResourceGroup resourceGroup, HashSet<ServiceType> services, string name, ISource source,
            List<StyleGroup> styleGroups, Envelope envelope = null) :
            base(name, source, styleGroups, envelope)
        {
            ResourceGroup = resourceGroup;
            Services = services;
        }

        public override string ToString()
        {
            return ResourceGroup == null ? Name : $"{ResourceGroup.Name}:{Name}";
        }

        public override ILayer Clone()
        {
            var styleGroups = new List<StyleGroup>();
            foreach (var styleGroup in StyleGroups)
            {
                styleGroups.Add(styleGroup.Clone());
            }

            return new LayerEntity(ResourceGroup, Services, Name, Source.Clone(), styleGroups, Envelope)
            {
                MinZoom = MinZoom,
                MaxZoom = MaxZoom,
                ZoomUnit = ZoomUnit,
                Enabled = Enabled,
                Buffers = Buffers
            };
        }
    }
}
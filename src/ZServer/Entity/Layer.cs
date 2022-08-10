using System.Collections.Generic;
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
        public HashSet<ServiceType> Services { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ResourceGroup ResourceGroup { get; private set; }

        public LayerEntity(ResourceGroup resourceGroup, HashSet<ServiceType> services, string name, ISource source,
            List<StyleGroup> styleGroups) :
            base(name, source, styleGroups)
        {
            ResourceGroup = resourceGroup;
            Services = services;
        }

        public override string ToString()
        {
            return ResourceGroup == null ? Name : $"{ResourceGroup.Name}:{Name}";
        }
    }
}
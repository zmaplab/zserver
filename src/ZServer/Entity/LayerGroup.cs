using System.Collections.Generic;
using ZMap;

namespace ZServer.Entity
{
    public class LayerGroup : IOgcWebServiceProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public ResourceGroup ResourceGroup { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 图层列表
        /// </summary>
        public List<ILayer> Layers { get; set; }

        public HashSet<ServiceType> Services { get; set; }
    }
}
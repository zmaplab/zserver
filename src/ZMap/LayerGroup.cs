using System.Collections.Generic;

namespace ZMap
{
    public class LayerGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public ResourceGroup ResourceGroup { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 图层列表
        /// </summary>
        public List<Layer> Layers { get; set; }

        public HashSet<ServiceType> Services { get; set; }

        public LayerGroup Clone()
        {
            var layerGroup = new LayerGroup
            {
                Name = Name,
                Services = Services,
                ResourceGroup = ResourceGroup,
                Layers = new List<Layer>()
            };

            if (Layers == null)
            {
                return layerGroup;
            }

            foreach (var layer in Layers)
            {
                layerGroup.Layers.Add(layer.Clone());
            }

            return layerGroup;
        }
    }
}
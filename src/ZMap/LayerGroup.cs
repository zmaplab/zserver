using System.Collections.Generic;

namespace ZMap;

public class LayerGroup
{
    /// <summary>
    /// 资源分组
    /// </summary>
    public ResourceGroup ResourceGroup { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 图层列表
    /// </summary>
    public List<Layer> Layers { get; set; }

    /// <summary>
    /// 开启的地图服务
    /// </summary>
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
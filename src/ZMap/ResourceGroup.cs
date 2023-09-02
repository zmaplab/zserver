using System;

namespace ZMap
{
    /// <summary>
    /// 资源分组
    /// </summary>
    public class ResourceGroup
    {
        /// <summary>
        /// 标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        public ResourceGroup Clone()
        {
            return (ResourceGroup)MemberwiseClone();
        }
    }
}
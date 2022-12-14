using System.Xml.Serialization;

namespace ZMap.SLD
{
    public class Label
    {
        /// <summary>
        /// 属性名
        /// </summary>
        [XmlElement("PropertyName")]
        public string PropertyName { get; set; }

        /// <summary>
        /// 扩展功能， 用于提供 C# 脚本， 动态编译
        /// </summary>
        [XmlElement("Expression")]
        public string Expression { get; set; }
    }
}
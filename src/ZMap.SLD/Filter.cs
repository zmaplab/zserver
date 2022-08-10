using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ZMap.SLD
{
    public class Filter
    {
        private static readonly Dictionary<string, IFilterDeserializer> Deserializers;

        static Filter()
        {
            Deserializers = typeof(Filter).Assembly.GetTypes().Where(x =>
                    !x.IsAbstract && !x.IsInterface && typeof(IFilterDeserializer).IsAssignableFrom(x))
                .ToDictionary(x => x.Name, x => (IFilterDeserializer)Activator.CreateInstance(x));
        }

        public string ReadXml(XmlReader reader)
        {
            var stack = new Stack<dynamic>();
            while (reader.Read())
            {
                var name = reader.LocalName;

                if (Deserializers.ContainsKey(name) && reader.NodeType == XmlNodeType.Element)
                {
                    var deserializer = Deserializers[name];
                    deserializer.Start(stack, reader);
                }

                if (Deserializers.ContainsKey(name) && reader.NodeType == XmlNodeType.EndElement)
                {
                    var deserializer = Deserializers[name];
                    deserializer.End(stack);
                }

                if (reader.LocalName == "Filter" && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            if (stack.Count != 1)
            {
                throw new Exception("SLD Filter 不符合规范");
            }

            return (string)stack.Pop().ToString();
        }
    }
}
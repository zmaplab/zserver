using System.Collections.Generic;

namespace ZServer
{
    public class QueryLayerParams
    {
        public QueryLayerParams(string resourceGroup, string layer, IDictionary<string, object> arguments = null)
        {
            Arguments = arguments;
            ResourceGroup = resourceGroup;
            Layer = layer;
        }

        public QueryLayerParams(string layer)
        {
            Layer = layer;
        }

        /// <summary>
        /// 工作区
        /// </summary>
        public string ResourceGroup { get; private set; }

        /// <summary>
        /// 图层
        /// </summary>
        public string Layer { get; private set; }

        /// <summary>
        /// 额外的参数， 如 WMS 的 CQL_FILTER
        /// </summary>
        public IDictionary<string, object> Arguments { get; private set; }
    }
}
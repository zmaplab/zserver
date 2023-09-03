using Orleans;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ZServer.Interfaces
{
    [GenerateSerializer, Immutable]
    public struct MapResult
    {
        /// <summary>
        /// 执行结果是否正确
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// 返回的消息
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// A service to indicate to a client where in the client's request an exception was encountered
        /// </summary>
        public string Locator { get; private set; }

        /// <summary>
        /// 内容格式
        /// </summary>
        public string ImageType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] ImageBytes { get; private set; }

        public static MapResult Failed(string message, string code, string locator = null)
        {
            return new MapResult
            {
                Success = false,
                Code = code,
                Locator = locator,
                Message = message,
                ImageBytes = null,
                ImageType = null
            };
        }

        // public static MapResult EmptyMap(string contentType)
        // {
        //     return new MapResult
        //     {
        //         Success = true,
        //         Code = "200",
        //         Message = null,
        //         ImageType = contentType,
        //         ImageBytes = Array.Empty<byte>()
        //     };
        // }

        public static MapResult Ok(byte[] bytes, string type)
        {
            return new MapResult
            {
                Success = true,
                Code = "200",
                Message = null,
                ImageType = type,
                ImageBytes = bytes
            };
        }
    }
}
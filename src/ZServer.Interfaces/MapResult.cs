using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ZServer.Interfaces
{
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

        public MapImage Image { get; private set; }

        public static MapResult Failed(string message, string code, string locator = null)
        {
            return new MapResult
            {
                Success = false,
                Code = code,
                Locator = locator,
                Message = message,
                Image = null
            };
        }

        public static MapResult EmptyMap(string contentType)
        {
            return new MapResult
            {
                Success = true,
                Code = "200",
                Message = null,
                Image = new MapImage(Array.Empty<byte>(), contentType)
            };
        }

        public static MapResult Ok(byte[] content, string contentType)
        {
            return new MapResult
            {
                Success = true,
                Code = "200",
                Message = null,
                Image = new MapImage(content, contentType)
            };
        }
    }
}
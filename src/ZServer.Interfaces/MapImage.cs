using Orleans.Concurrency;

namespace ZServer.Interfaces
{
    public record MapImage
    {
        public MapImage(byte[] content, string contentType)
        {
            Content = content.AsImmutable();
            ContentType = contentType;
        }

        /// <summary>
        /// 内容格式
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Immutable<byte[]> Content { get; private set; }
    }
}
using System.Xml.Serialization;

namespace ZServer.Interfaces
{
    [XmlRoot("ServerException")]
    public class ServerException
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("code")]
        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("locator")]
        public string Locator { get; set; }

        public ServerException()
        {
        }

        public ServerException(string text, string code)
        {
            Text = text;
            Code = code;
        }
    }
}
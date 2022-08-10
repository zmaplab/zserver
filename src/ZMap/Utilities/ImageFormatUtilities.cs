namespace ZMap.Utilities
{
    public static class ImageFormatUtilities
    {
        /// <summary>
        /// TODO: more extension names
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetExtension(string format)
        {
            return format switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpeg",
                _ => ".png"
            };
        }
    }
}
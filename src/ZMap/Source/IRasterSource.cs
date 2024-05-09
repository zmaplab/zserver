namespace ZMap.Source;

public interface IRasterSource : ISource
{
    /// <summary>
    /// 数据源读出的数据，需要转码成图片
    /// </summary>
    /// <param name="extent"></param>
    /// <returns></returns>
    Task<byte[]> GetImageInExtentAsync(Envelope extent);
}
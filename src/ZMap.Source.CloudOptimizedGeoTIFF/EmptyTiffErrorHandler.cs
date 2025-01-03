using BitMiracle.LibTiff.Classic;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public class EmptyTiffErrorHandler : TiffErrorHandler
{
    public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
    {
    }

    public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format,
        params object[] args)
    {
    }
}
using System.Text;
using BitMiracle.LibTiff.Classic;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public static class GeoInfo
{
    /// <summary>
    /// 1024 1 表示投影坐标系，2 表示地理经纬系，3 表示地心(X,Y,Z)坐标系
    /// 2048 地理类型代码，指出采用哪一个地理坐标系统。值为 32767 表示为自定义的坐标系。
    /// </summary>
    /// <param name="tiff"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static Dictionary<int, GeoKeyEntry> Read(Tiff tiff)
    {
        var geoKeyDirectory = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        var geoDoubleParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];
        var geoAsciiParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];

        var doubleParams = new double[geoDoubleParamsFieldValue.Length / 8];
        for (var i = 0; i < doubleParams.Length; ++i)
        {
            doubleParams[i] = BitConverter.ToDouble(geoDoubleParamsFieldValue, i * 8);
        }

        var bytes = geoKeyDirectory[1].ToByteArray();
        // var v1 = BitConverter.ToUInt16(bytes, 0);
        // var v2 = BitConverter.ToUInt16(bytes, 2);
        // var v3 = BitConverter.ToUInt16(bytes, 4);
        var numGeoKeys = BitConverter.ToUInt16(bytes, 6);

        var geoKeys = new Dictionary<int, GeoKeyEntry>();
        var start = 8;
        for (var i = 0; i < numGeoKeys; i++)
        {
            if (bytes.Length <= start + 8)
            {
                break;
            }

            var keyId = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var location = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var count = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var offset = BitConverter.ToUInt16(bytes, start);
            start += 2;
            var entry = new GeoKeyEntry
            {
                KeyId = keyId,
                TiffTagLocation = location,
                Count = count,
                ValueOffset = offset
            };
            if (location != 0)
            {
                var end = count - 1 + offset;
                switch (location)
                {
                    case 34737:
                    {
                        var piece = Encoding.ASCII.GetString(geoAsciiParamsFieldValue[offset..end]).Split('=');
                        entry.Value = piece.Length == 1 ? piece[0] : piece[1].Trim();
                        break;
                    }
                    case 34736:
                    {
                        entry.Value = doubleParams[offset];
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException("不支持的数据地址");
                    }
                }
            }
            else
            {
                entry.Value = offset;
            }

            // (keyId, location, count, value)
            geoKeys.Add(keyId, entry);
        }

        return geoKeys;
    }
}
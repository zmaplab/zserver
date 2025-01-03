using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitMiracle.LibTiff.Classic;

namespace ZMap.Source.CloudOptimizedGeoTIFF;

public static class GeoInfo
{
    public static Dictionary<int, (int KeyId, int Location, int Count, object Value)> Read(Tiff tiff)
    {
        var geoKeyDirectory = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        var geoDoubleParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];
        var geoAsciiParamsFieldValue =
            tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG)?.ElementAtOrDefault(1).Value as byte[] ?? [];

        var doubleParams = new double[geoDoubleParamsFieldValue.Length / 4];
        for (var i = 0; i < doubleParams.Length / 4; ++i)
        {
            doubleParams[i] = BitConverter.ToDouble(geoDoubleParamsFieldValue, i * 4);
        }

        var bytes = geoKeyDirectory[1].ToByteArray();
        // var v1 = BitConverter.ToUInt16(bytes, 0);
        // var v2 = BitConverter.ToUInt16(bytes, 2);
        // var v3 = BitConverter.ToUInt16(bytes, 4);
        var numGeoKeys = BitConverter.ToUInt16(bytes, 6);

        var geoKeys = new Dictionary<int, (int KeyId, int Location, int Count, object Value)>();
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

            object value;
            if (location != 0)
            {
                var end = count - 1 + offset;
                switch (location)
                {
                    case 34737:
                    {
                        var piece = Encoding.ASCII.GetString(geoAsciiParamsFieldValue[offset..end]).Split('=');
                        value = piece.Length == 1 ? piece[0] : piece[1].Trim();
                        break;
                    }
                    case 34736:
                    {
                        value = doubleParams[offset];
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
                value = offset;
            }

            geoKeys.Add(keyId, (keyId, location, count, value));
        }

        return geoKeys;
    }
}
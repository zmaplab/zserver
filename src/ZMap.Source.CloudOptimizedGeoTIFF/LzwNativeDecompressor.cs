using ZMap.Source.CloudOptimizedGeoTIFF;

/// <summary>
/// Compression and decompression support for LZW algorithm.
/// </summary>
public class LzwCompressionAlgorithm
{
    /// <summary>
    /// A shared instance of <see cref="LzwCompressionAlgorithm"/>. It should be used across the application.
    /// </summary>
    public static LzwCompressionAlgorithm Instance { get; } = new LzwCompressionAlgorithm();

    public int Decompress(Span<byte> input, Stream output)
    {
        ReadOnlySpan<byte> inputSpan = input;
     
        if (inputSpan.IsEmpty)
        {
            return 0;
        }

        var first = inputSpan[0];
        return first switch
        {
            0 => DecompressLeastSignificantBitFirst(inputSpan, output),
            128 => DecompressMostSignificantBitFirst(inputSpan, output),
            _ => throw new InvalidDataException()
        };
    }

    private static int DecompressLeastSignificantBitFirst(ReadOnlySpan<byte> input, Stream stream)
    {
        var lzw = new TiffLzwDecoderLeastSignificantBitFirst();
        try
        {
            lzw.Initialize();
            return lzw.Decode(input, stream);
        }
        finally
        {
            lzw.Dispose();
        }
    }

    private static int DecompressMostSignificantBitFirst(ReadOnlySpan<byte> input, Stream stream)
    {
        var lzw = new TiffLzwDecoderMostSignificantBitFirst();
        try
        {
            lzw.Initialize();
            return lzw.Decode(input, stream);
        }
        finally
        {
            lzw.Dispose();
        }
    }
}
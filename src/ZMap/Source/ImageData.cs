namespace ZMap.Source;

public class ImageData(Array data, ImageDataType type, int width = 0, int height = 0)
{
    public Array Data { get; private set; } = data;
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;
    public ImageDataType Type { get; private set; } = type;
    public bool IsEmpty { get; private set; } = data == null || data.Length == 0;
}
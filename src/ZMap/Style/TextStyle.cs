namespace ZMap.Style
{
    public class TextStyle : VectorStyle
    {
        public Expression<string> Property { get; set; }
        public Expression<string> Color { get; set; }
        public Expression<string[]> Font { get; set; }
        public Expression<int> Size { get; set; }
        public Expression<string> Align { get; set; }
        public Expression<float> Rotate { get; set; }
        public Expression<TextTransform> Transform { get; set; }
        public Expression<float[]> Offset { get; set; }
    }
}
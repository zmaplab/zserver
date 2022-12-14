namespace ZMap.Style
{
    public class SpriteFillStyle : ResourceFillStyle
    {
        public Expression<string> Pattern { get; set; }
        
        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Pattern?.Invoke(feature);
        }
    }
}
namespace ZMap.Style
{
    public class SpriteFillStyle : ResourceFillStyle
    {
        public override void Accept(IZMapStyleVisitor visitor, Feature feature)
        {
            base.Accept(visitor, feature);

            Pattern?.Invoke(feature);
        }
    }
}
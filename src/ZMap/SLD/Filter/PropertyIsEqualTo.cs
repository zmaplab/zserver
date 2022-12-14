namespace ZMap.SLD.Filter
{
    public class PropertyIsEqualTo : BinaryComparisonOperator
    {
        public override object Accept(IFilterVisitor visitor, object extraData)
        {
            return visitor.Visit(this, extraData);
        }
    }
}
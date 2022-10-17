namespace ZMap.SLD.Filter
{
    public class PropertyIsBetween : Filter
    {
        public Expression.Expression Expression { get; set; }

        public Expression.Expression LowerBoundary { get; set; }

        public Expression.Expression UpperBoundary { get; set; }

        public override bool Evaluate(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override void Accept(FilterVisitor visitor, object extraData)
        {
            throw new System.NotImplementedException();
        }
    }
}
namespace ZMap.SLD.Filter;

public interface IFilterVisitor : IExpressionVisitor
{
    object Visit(PropertyIsEqualToType filter, object data);
    object Visit(LogicOpsType logicOpsType, object data);
    object Visit(ComparisonOpsType comparisonOpsType, object data);
    object Visit(UpperBoundaryType upperBoundaryType, object data);
    object Visit(And and, object data);
    object Visit(PropertyIsGreaterThanOrEqualTo filter, object data);
    object Visit(PropertyIsLessThan filter, object data);
    object VisitObject(object obj, object data);
}
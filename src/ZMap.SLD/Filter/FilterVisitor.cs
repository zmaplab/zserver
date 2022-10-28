// using ZMap.SLD.Expression;
//
// namespace ZMap.SLD.Filter;
//
// public class FilterVisitor
// {
//     private readonly IExpressionVisitor _expressionVisitor;
//
//     public FilterVisitor(IExpressionVisitor expressionVisitor)
//     {
//         this._expressionVisitor = expressionVisitor;
//     }
//
//     public object Visit(PropertyIsEqualTo filter, object data)
//     {
//         return Visit((BinaryComparisonOperator)filter, data);
//     }
//
//     private object Visit(BinaryComparisonOperator filter, object data)
//     {
//         if (_expressionVisitor == null)
//         {
//             return filter;
//         }
//
//         filter.Left?.Accept(_expressionVisitor, data);
//         filter.Right?.Accept(_expressionVisitor, data);
//
//         return filter;
//     }
// }
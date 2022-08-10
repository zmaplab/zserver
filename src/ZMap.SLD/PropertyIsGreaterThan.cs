using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public class PropertyIsGreaterThan
        : IFilterDeserializer
    {
        private dynamic _expression1;
        private dynamic _expression2;

        public void Start(Stack<dynamic> stack, XmlReader reader)
        {
        }

        public void End(Stack<dynamic> stack)
        {
            stack.Push(new PropertyIsGreaterThan
            {
                _expression2 = stack.Pop(),
                _expression1 = stack.Pop()
            });
        }

        public override string ToString()
        {
            var propertyIsEqualTo = _expression1 is PropertyName
                ? $"feature.PropertyIsGreaterThan({_expression1}, {_expression2})"
                : $"feature.PropertyIsGreaterThan({_expression2}, {_expression1})";
            return propertyIsEqualTo;
        }
    }
}
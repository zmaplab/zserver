using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD
{
    public class PropertyIsBetween : IFilterDeserializer
    {
        private dynamic _expression1;
        private dynamic _expression2;
        private dynamic _expression3;

        public void Start(Stack<dynamic> stack, XmlReader reader)
        {
        }

        public void End(Stack<dynamic> stack)
        {
            stack.Push(new PropertyIsBetween
            {
                _expression3 = stack.Pop(),
                _expression2 = stack.Pop(),
                _expression1 = stack.Pop()
            });
        }

        public override string ToString()
        {
            return $"feature.PropertyIsBetween({_expression1}, {_expression2}, {_expression3})";
        }
    }
}
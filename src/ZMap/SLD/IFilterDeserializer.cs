using System.Collections.Generic;
using System.Xml;

namespace ZMap.SLD;

public interface IFilterDeserializer
{
    void Start(Stack<dynamic> stack, XmlReader reader);
    void End(Stack<dynamic> stack);
}
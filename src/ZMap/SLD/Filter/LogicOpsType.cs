using System;
using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

[XmlInclude(typeof(UnaryLogicOpType))]
[XmlInclude(typeof(BinaryLogicOpType))]
public abstract class LogicOpsType
{
    public virtual object Accept(IFilterVisitor visitor, object extraData)
    {
        throw new NotImplementedException();
    }
}
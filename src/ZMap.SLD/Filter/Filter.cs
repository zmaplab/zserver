using System;
using System.Xml.Serialization;

namespace ZMap.SLD.Filter;

public abstract class Filter
{
    public virtual bool Evaluate(object obj)
    {
        return false;
    }

    public abstract object Accept(IFilterVisitor visitor, object extraData);
}
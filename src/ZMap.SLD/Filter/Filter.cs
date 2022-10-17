using System;

namespace ZMap.SLD.Filter;

public abstract class Filter
{
    public abstract bool Evaluate(object obj);

    public abstract void Accept(FilterVisitor visitor, object extraData);
}
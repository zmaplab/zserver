namespace ZMap.SLD;

public abstract class Constant
    : IFilterDeserializer
{
    public string Value { get; private set; }

    public override string ToString()
    {
        return Value;
    }

    public void Start(Stack<dynamic> stack, XmlReader reader)
    {
        var constant = (Constant)MemberwiseClone();
        constant.Value = $"\"{reader.ReadString()}\"";
        stack.Push(constant);
    }

    public void End(Stack<dynamic> stack)
    {
    }
}